using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Twilio.Security;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Class used to validate incoming requests from Twilio using 'Request Validation' as described
    /// in the Security section of the Twilio TwiML API documentation.
    /// </summary>
    public class RequestValidationHelper
    {
        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public bool IsValidRequest(HttpContext context, string authToken, bool allowLocal = true)
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
        public bool IsValidRequest(HttpContext context, string authToken, string urlOverride, bool allowLocal = true)
        {
            var request = context.Request;

            if (allowLocal && request.IsLocal())
            {
                return true;
            }

            // validate request
            // http://www.twilio.com/docs/security-reliability/security
            // Take the full URL of the request, from the protocol (http...) through the end of the query string (everything after the ?)
            string fullUrl = string.IsNullOrEmpty(urlOverride)
                ? $"{request.Scheme}://{(request.IsHttps ? request.Host.Host : request.Host.ToUriComponent())}{request.Path}{request.QueryString}"
                : urlOverride;

            IFormCollection form = null;
            try
            {
                form = request.Form;
            }
            catch
            {
                // ignore errors accessing invalid/non-existant form
            }
            
            var validator = new RequestValidator(authToken);
            return validator.Validate(
                url: fullUrl,
                parameters: form?.Count > 0
                    ? form.Keys.ToDictionary(k => k, k => form[k].ToString())
                    : new Dictionary<string, string>(),
                expected: request.Headers["X-Twilio-Signature"]
            );
        }
    }
}
