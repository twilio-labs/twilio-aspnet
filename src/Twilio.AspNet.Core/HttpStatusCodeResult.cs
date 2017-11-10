using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Returns an HTTP status code
    /// </summary>
    public class HttpStatusCodeResult : IActionResult
    {
        /// <summary>
        /// Creates a new instance of the class with a specific status code
        /// </summary>
        /// <param name="statusCode">The status code to return</param>
        public HttpStatusCodeResult(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
        }

        /// <summary>
        /// Gets the status code for this instance
        /// </summary>
        public int StatusCode { get; private set; }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.HttpContext.Response.StatusCode = StatusCode;

            return Task.CompletedTask;
        }
    }
}
