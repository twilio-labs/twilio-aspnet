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
        private bool? allowLocal = null;

        public string AuthToken { get; set; }
        public string UrlOverride { get; set; }

        // by storing AllowLocal of bool into allowLocal of bool?
        // we know that when allowLocal != null, the user explicitly configured AllowLocal,
        // thus we should use the value of allowLocal, and not consider TwilioOptions
        // (type of bool? is not allowed as attribute parameters which is why we need this workaround)
        public bool AllowLocal
        {
            set => allowLocal = value;
        }

        public bool IsReusable => true;

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        public ValidateRequestAttribute()
        {
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var twilioOptions = serviceProvider.GetService<IOptions<TwilioOptions>>()?.Value;
            if (twilioOptions == null)
            {
                return new InternalValidateRequestAttribute(
                    authToken: AuthToken,
                    urlOverride: UrlOverride,
                    allowLocal: allowLocal ?? true
                );
            }

            var requestValidationOptions = twilioOptions.RequestValidation;
            return new InternalValidateRequestAttribute(
                authToken: AuthToken ?? requestValidationOptions?.AuthToken,
                urlOverride: UrlOverride ?? requestValidationOptions?.UrlOverride,
                allowLocal: allowLocal ?? requestValidationOptions?.AllowLocal ?? true
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