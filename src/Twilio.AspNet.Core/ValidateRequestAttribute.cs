using System.Net;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Represents an attribute that is used to prevent forgery of a request.
    /// </summary>
	public class ValidateRequestAttribute : ActionFilterAttribute
    {
        public string AuthToken { get; set; }
        public string UrlOverride { get; set; }
        public bool AllowLocal { get; set; }

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
		public ValidateRequestAttribute(string authToken, bool allowLocal = true)
        {
            AuthToken = authToken;
            AllowLocal = allowLocal;
        }

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
		public ValidateRequestAttribute(string authToken, string urlOverride, bool allowLocal = true)
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