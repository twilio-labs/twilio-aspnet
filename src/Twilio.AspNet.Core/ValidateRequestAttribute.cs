using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Twilio.AspNet.Core;

/// <summary>
/// Represents an attribute that is used to prevent forgery of a request.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ValidateRequestAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext filterContext,
        ActionExecutionDelegate next
    )
    {
        var context = filterContext.HttpContext;
        if (await RequestValidationHelper.IsValidRequestAsync(context).ConfigureAwait(false))
        {
            await next();
            return;
        }

        filterContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
    }
}