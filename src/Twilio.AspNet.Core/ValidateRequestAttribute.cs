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
        public string AuthToken { get; set; }
        public string UrlOverride { get; set; }
        public bool AllowLocal { get; set; }

        public bool IsReusable => true;

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        public ValidateRequestAttribute()
            : this(null, null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public ValidateRequestAttribute(string authToken = null, bool allowLocal = true)
            : this(authToken, null, allowLocal)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public ValidateRequestAttribute(string authToken = null, string urlOverride = null, bool allowLocal = true)
        {
            AuthToken = authToken;
            UrlOverride = urlOverride;
            AllowLocal = allowLocal;
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var twilioOptions = serviceProvider.GetService<IOptions<TwilioOptions>>();
            return new InternalValidateRequestAttribute(
                authToken: AuthToken ?? twilioOptions?.Value.AuthToken,
                urlOverride: UrlOverride,
                allowLocal: AllowLocal
            );
        }

        private class InternalValidateRequestAttribute : ActionFilterAttribute
        {
            private readonly string authToken;
            private readonly string urlOverride;
            private readonly bool allowLocal;

            /// <summary>
            /// Initializes a new instance of the ValidateRequestAttribute class.
            /// </summary>
            /// <param name="authToken">AuthToken for the account used to sign the request</param>
            /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
            /// <param name="allowLocal">Skip validation for local requests</param>
            public InternalValidateRequestAttribute(string authToken, string urlOverride, bool allowLocal)
            {
                this.authToken = authToken;
                this.urlOverride = urlOverride;
                this.allowLocal = allowLocal;
            }

            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                var validator = new RequestValidationHelper();

                if (!validator.IsValidRequest(filterContext.HttpContext, authToken, urlOverride, allowLocal))
                {
                    filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                base.OnActionExecuting(filterContext);
            }
        }
    }
}