using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Twilio.Security;

namespace Twilio.AspNet.Core
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
        public static Task<bool> IsValidRequestAsync(HttpContext context, string authToken, bool allowLocal = true)
            => IsValidRequestAsync(context, authToken, null, allowLocal);

        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public static async Task<bool> IsValidRequestAsync(
            HttpContext context, 
            string authToken, 
            string urlOverride, 
            bool allowLocal = true
        )
        {
            var request = context.Request;

            if (allowLocal && IsLocal(request))
            {
                return true;
            }

            // validate request
            // http://www.twilio.com/docs/security-reliability/security
            // Take the full URL of the request, from the protocol (http...) through the end of the query string (everything after the ?)
            string fullUrl = string.IsNullOrEmpty(urlOverride)
                ? $"{request.Scheme}://{(request.IsHttps ? request.Host.Host : request.Host.ToUriComponent())}{request.Path}{request.QueryString}"
                : urlOverride;

            Dictionary<string, string> parameters = null;
            if (request.HasFormContentType)
            {
                var form = await request.ReadFormAsync(context.RequestAborted).ConfigureAwait(false);
                parameters = form.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
            }

            var validator = new RequestValidator(authToken);
            return validator.Validate(
                url: fullUrl,
                parameters: parameters,
                expected: request.Headers["X-Twilio-Signature"]
            );
        }
        
        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public static bool IsValidRequest(HttpContext context, string authToken, bool allowLocal = true)
            => IsValidRequest(context, authToken, null, allowLocal);

        /// <summary>
        /// Performs request validation using the current HTTP context passed in manually or from
        /// the ASP.NET MVC ValidateRequestAttribute
        /// </summary>
        /// <param name="context">HttpContext to use for validation</param>
        /// <param name="authToken">AuthToken for the account used to sign the request</param>
        /// <param name="urlOverride">The URL to use for validation, if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)</param>
        /// <param name="allowLocal">Skip validation for local requests</param>
        public static bool IsValidRequest(
            HttpContext context, 
            string authToken, 
            string urlOverride, 
            bool allowLocal = true
        )
        {
            var request = context.Request;

            if (allowLocal && IsLocal(request))
            {
                return true;
            }

            // validate request
            // http://www.twilio.com/docs/security-reliability/security
            // Take the full URL of the request, from the protocol (http...) through the end of the query string (everything after the ?)
            string fullUrl = string.IsNullOrEmpty(urlOverride)
                ? $"{request.Scheme}://{(request.IsHttps ? request.Host.Host : request.Host.ToUriComponent())}{request.Path}{request.QueryString}"
                : urlOverride;

            Dictionary<string, string> parameters = null;
            if (request.HasFormContentType)
            {
                parameters = request.Form.ToDictionary(kv => kv.Key, kv => kv.Value.ToString());
            }

            var validator = new RequestValidator(authToken);
            return validator.Validate(
                url: fullUrl,
                parameters: parameters,
                expected: request.Headers["X-Twilio-Signature"]
            );
        }

        private static bool IsLocal(HttpRequest req)
        {
            if (req.Headers.ContainsKey("X-Forwarded-For"))
            {
                // Assume not local if we're behind a proxy
                return false;
            }

            var connection = req.HttpContext.Connection;
            if (connection.RemoteIpAddress != null)
            {
                if (connection.LocalIpAddress != null)
                {
                    return connection.RemoteIpAddress.Equals(connection.LocalIpAddress);
                }

                return IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            // for in memory TestServer or when dealing with default connection info
            if (connection.RemoteIpAddress == null && connection.LocalIpAddress == null)
            {
                return true;
            }

            return false;
        }
    }
}