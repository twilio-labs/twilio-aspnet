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
                urlOverride: options.UrlOverride,
                allowLocal: options.AllowLocal ?? true
            );
        }

        internal class InternalValidateRequestAttribute : ActionFilterAttribute
        {
            internal string AuthToken { get; set; }
            internal string UrlOverride { get; set; }
            internal bool AllowLocal { get; set; }

            /// <summary>
            /// Initializes a new instance of the ValidateRequestAttribute class.
            /// </summary>
            /// <param name="authToken">AuthToken for the account used to sign the request</param>
            /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
            /// <param name="allowLocal">Skip validation for local requests</param>
            public InternalValidateRequestAttribute(string authToken, string urlOverride, bool allowLocal)
            {
                AuthToken = authToken;
                UrlOverride = urlOverride;
                AllowLocal = allowLocal;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var validator = new RequestValidationHelper();

                if (!validator.IsValidRequest(filterContext.HttpContext, AuthToken, UrlOverride, AllowLocal))
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                base.OnActionExecuting(filterContext);
            }
        }
    }
}