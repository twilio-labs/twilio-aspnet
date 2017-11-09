using System.Net;
using System.Web.Mvc;

namespace Twilio.AspNet.Mvc
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
        /// <param name="authToken"></param>
		public ValidateRequestAttribute(string authToken, bool allowLocal = true)
		{
			AuthToken = authToken;
            AllowLocal = allowLocal;
		}

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="urlOverride"></param>
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