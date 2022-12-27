using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Validates that incoming HTTP request originates from Twilio.
    /// </summary>
    public class ValidateTwilioRequestMiddleware
    {
        private readonly RequestDelegate next;
        private readonly TwilioRequestValidationOptions options;

        public ValidateTwilioRequestMiddleware(
            RequestDelegate next,
            IOptions<TwilioRequestValidationOptions> options
        )
        {
            this.next = next;
            this.options = options.Value ?? throw new Exception("RequestValidationOptions is not configured.");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;

            string urlOverride = null;
            if (options.BaseUrlOverride != null)
            {
                urlOverride = $"{options.BaseUrlOverride.TrimEnd('/')}{request.Path}{request.QueryString}";
            }

            var isValid = await RequestValidationHelper.IsValidRequestAsync(context, options.AuthToken, urlOverride, options.AllowLocal ?? true);
            if (!isValid)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
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