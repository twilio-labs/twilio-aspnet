using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Validates that incoming HTTP request originates from Twilio.
    /// </summary>
    public class ValidateTwilioRequestMiddleware
    {
        private readonly RequestDelegate next;

        public ValidateTwilioRequestMiddleware(RequestDelegate next)
        {
            this.next = next;
        }
        
        public async Task InvokeAsync(HttpContext context)
        {
            if (!RequestValidationHelper.IsValidRequest(context))
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            await next(context);
        }
    }

    public static class ValidateTwilioRequestMiddlewareExtensions
    {
        /// <summary>
        /// Validates that incoming HTTP request originates from Twilio.
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseTwilioRequestValidation(this IApplicationBuilder builder)
            => builder.UseMiddleware<ValidateTwilioRequestMiddleware>();
    }
}