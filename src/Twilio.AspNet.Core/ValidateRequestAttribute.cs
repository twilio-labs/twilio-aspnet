using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Represents an attribute that is used to prevent forgery of a request.
    /// </summary>
    public class ValidateRequestAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => true;

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
        public ValidateRequestAttribute()
        {
        }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<TwilioRequestValidationOptions>>()?.Value;
            if (options == null) throw new Exception("RequestValidationOptions is not configured.");

            return new InternalValidateRequestFilter(
                authToken: options.AuthToken,
                baseUrlOverride: options.BaseUrlOverride?.TrimEnd('/'),
                allowLocal: options.AllowLocal ?? true
            );
        }

        internal class InternalValidateRequestFilter : IAsyncActionFilter
        {
            internal string AuthToken { get; }
            internal string BaseUrlOverride { get; }
            internal bool AllowLocal { get; }

            /// <summary>
            /// Initializes a new instance of the ValidateRequestAttribute class.
            /// </summary>
            /// <param name="authToken">AuthToken for the account used to sign the request</param>
            /// <param name="baseUrlOverride">
            /// The Base URL (protocol + hostname) to use for validation,
            /// if different from Request.Url (sometimes needed if web site is behind a proxy or load-balancer)
            /// </param>
            /// <param name="allowLocal">Skip validation for local requests</param>
            public InternalValidateRequestFilter(string authToken, string baseUrlOverride, bool allowLocal)
            {
                AuthToken = authToken;
                BaseUrlOverride = baseUrlOverride;
                AllowLocal = allowLocal;
            }

            public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                var httpContext = context.HttpContext;
                var request = httpContext.Request;
                string urlOverride = null;
                if (BaseUrlOverride != null)
                {
                    urlOverride = $"{BaseUrlOverride}{request.Path}{request.QueryString}";
                }

                var isValid = await RequestValidationHelper
                    .IsValidRequestAsync(httpContext, AuthToken, urlOverride, AllowLocal).ConfigureAwait(false);
                if (!isValid)
                {
                    context.Result = new StatusCodeResult((int) HttpStatusCode.Forbidden);
                    return;
                }

                await next();
            }
        }
    }
}