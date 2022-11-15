using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Twilio.Security;

namespace Twilio.AspNet.Mvc
{
	/// <summary>
	/// Class used to validate incoming requests from Twilio using 'Request Validation' as described
	/// in the Security section of the Twilio TwiML API documentation.
	/// </summary>
	public static class RequestValidationHelper
    {
        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public static bool IsValidRequest(HttpContextBase context, string authToken, bool allowLocal = true)
        {
            return IsValidRequest(context, authToken, null, allowLocal);
        }

        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public static bool IsValidRequest(HttpContextBase context, string authToken, string urlOverride, bool allowLocal = true)
        {
            if (allowLocal && context.Request.IsLocal && !context.Request.Headers.AllKeys.Contains("X-Forwarded-For"))
            {
                return true;
            }

            var fullUrl = string.IsNullOrEmpty(urlOverride) ? context.Request.Url?.AbsoluteUri : urlOverride;
            var validator = new RequestValidator(authToken);
            return validator.Validate(
                url: fullUrl,
                parameters: context.Request?.Form ?? new NameValueCollection(),
                expected: context.Request?.Headers["X-Twilio-Signature"]
            );
        }
    }
}
