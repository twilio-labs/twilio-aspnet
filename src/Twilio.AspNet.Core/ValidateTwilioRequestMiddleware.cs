using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

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
            var options = context.RequestServices.GetRequiredService<IOptionsSnapshot<TwilioRequestValidationOptions>>().Value;
            if (options == null) throw new Exception("TwilioRequestValidationOptions is not configured.");
            var authToken = options.AuthToken;
            var baseUrlOverride = options.BaseUrlOverride;
            var allowLocal = options.AllowLocal ?? true;
            
            var request = context.Request;

            string urlOverride = null;
            if (baseUrlOverride != null)
            {
                urlOverride = $"{baseUrlOverride.TrimEnd('/')}{request.Path}{request.QueryString}";
            }

            var isValid = RequestValidationHelper.IsValidRequest(context, authToken, urlOverride, allowLocal);
            if (!isValid)
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