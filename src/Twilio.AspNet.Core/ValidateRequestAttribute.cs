using System;
using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Represents an attribute that is used to prevent forgery of a request.
    /// </summary>
    public class ValidateRequestAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        public ValidateRequestAttribute()
        {
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<TwilioRequestValidationOptions>>()?.Value;
            if (options == null) throw new Exception("RequestValidationOptions is not configured.");

            return new InternalValidateRequestAttribute(
                authToken: options.AuthToken,
                baseUrlOverride: options.BaseUrlOverride?.TrimEnd('/'),
                allowLocal: options.AllowLocal ?? true
            );
        }

        internal class InternalValidateRequestAttribute : ActionFilterAttribute
        {
            internal string AuthToken { get; set; }
            internal string BaseUrlOverride { get; set; }
            internal bool AllowLocal { get; set; }

            /// <summary>
            /// Initializes a new instance of the ValidateRequestAttribute class.
            /// </summary>
            /// <param name="authToken">AuthToken for the account used to sign the request</param>
            /// <param name="baseUrlOverride">
            /// The Base URL (protocol + hostname) to use for validation,
            /// if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)
            /// </param>
            /// <param name="allowLocal">Skip validation for local requests</param>
            public InternalValidateRequestAttribute(string authToken, string baseUrlOverride, bool allowLocal)
            {
                AuthToken = authToken;
                BaseUrlOverride = baseUrlOverride;
                AllowLocal = allowLocal;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var httpContext = filterContext.HttpContext;
                var request = httpContext.Request;
                string urlOverride = null;
                if (BaseUrlOverride != null)
                {
                    urlOverride = $"{BaseUrlOverride}{request.Path}{request.QueryString}";
                }
                
                if (!RequestValidationHelper.IsValidRequest(httpContext, AuthToken, urlOverride, AllowLocal))
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                base.OnActionExecuting(filterContext);
            }
        }
    }
}
