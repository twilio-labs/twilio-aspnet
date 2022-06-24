using System;
using System.Configuration;
using System.Net;
using System.Web.Mvc;

namespace Twilio.AspNet.Mvc
{
    /// <summary>
    /// Represents an attribute that is used to prevent forgery of a request.
    /// </summary>
	public class ValidateRequestAttribute : ActionFilterAttribute
    {
        protected internal string AuthToken { get; set; }
        protected internal string UrlOverride { get; set; }
        protected internal bool AllowLocal { get; set; }

        /// <summary>
        /// Initializes a new instance of the ValidateRequestAttribute class.
        /// </summary>
		public ValidateRequestAttribute()
        {
            ConfigureProperties();
        }

        /// <summary>
        /// Configures the properties of the attribute.
        /// </summary>
        /// <remarks>
        /// This method exists so ValidateRequestAttribute can be inherited from 
        /// and ConfigureProperties can be overridden.
        /// </remarks>
        /// <exception cref="Exception"></exception>
        protected virtual void ConfigureProperties()
        {
            var requestValidationConfiguration = ConfigurationManager.GetSection("twilio/requestValidation") as RequestValidationConfigurationSection;
            var appSettings = ConfigurationManager.AppSettings;

            AuthToken = appSettings["twilio:requestValidation:authToken"]
                ?? appSettings["twilio:authToken"]
                ?? requestValidationConfiguration?.AuthToken
                ?? throw new Exception("Twilio Auth Token not configured");

            UrlOverride = appSettings["twilio:requestValidation:urlOverride"]
                ?? requestValidationConfiguration?.AuthToken;

            var allowLocalAppSetting = appSettings["twilio:requestValidation:allowLocal"];
            AllowLocal = allowLocalAppSetting != null
                ? bool.Parse(allowLocalAppSetting)
                : requestValidationConfiguration?.AllowLocal
                ?? true;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var validator = new RequestValidationHelper();

            if (!validator.IsValidRequest(filterContext.HttpContext, AuthToken, UrlOverride, AllowLocal))
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}