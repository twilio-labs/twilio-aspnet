using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Represents an attribute that is used to prevent forgery of a request.
    /// </summary>
    public class ValidateRequestAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var context = filterContext.HttpContext;
            var options = context.RequestServices
                .GetRequiredService<IOptionsSnapshot<TwilioRequestValidationOptions>>().Value;
            
            var authToken = options.AuthToken;
            var baseUrlOverride = options.BaseUrlOverride;
            var allowLocal = options.AllowLocal ?? true;

            var request = context.Request;
            string urlOverride = null;
            if (baseUrlOverride != null)
            {
                urlOverride = $"{baseUrlOverride}{request.Path}{request.QueryString}";
            }

            if (!RequestValidationHelper.IsValidRequest(context, authToken, urlOverride, allowLocal))
            {
                filterContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}