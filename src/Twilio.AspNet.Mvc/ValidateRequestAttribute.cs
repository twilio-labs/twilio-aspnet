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

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken"></param>
		public ValidateRequestAttribute(string authToken)
		{
			AuthToken = authToken;
		}

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="urlOverride"></param>
		public ValidateRequestAttribute(string authToken, string urlOverride)
		{
			AuthToken = authToken;
			UrlOverride = urlOverride;
		}

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var validator = new RequestValidationHelper();

            if (!validator.IsValidRequest(filterContext.HttpContext, AuthToken, UrlOverride))
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            base.OnActionExecuting(filterContext);
        }

    }
}