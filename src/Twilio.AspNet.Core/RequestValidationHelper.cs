using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

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
            var requestedUrl = $"{request.Scheme}://{request.Host.Host}{request.Path}{request.QueryString}";
            var fullUrl = string.IsNullOrEmpty(urlOverride) ? requestedUrl : urlOverride;

            var value = new StringBuilder();
            value.Append(fullUrl);

            // If the request is a POST, take all of the POST parameters and sort them alphabetically.
            if (request.Method == "POST")
            {
                // Iterate through that sorted list of POST parameters, and append the variable name and value (with no delimiters) to the end of the URL string
                var sortedKeys = request.Form.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
                foreach (var key in sortedKeys)
                {
                    value.Append(key);
                    value.Append(request.Form[key]);
                }
            }

            // Sign the resulting value with HMAC-SHA1 using your AuthToken as the key (remember, your AuthToken's case matters!).
            var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(authToken));
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value.ToString()));

            // Base64 encode the hash
            var encoded = Convert.ToBase64String(hash);

            // Compare your hash to ours, submitted in the X-Twilio-Signature header. If they match, then you're good to go.
            var sig = request.Headers["X-Twilio-Signature"];

            return sig == encoded;
        }
    }
}
