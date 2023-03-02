using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

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
            if (!RequestValidationHelper.IsValidRequest(context))
            {
                filterContext.Result = new StatusCodeResult((int)HttpStatusCode.Forbidden);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}