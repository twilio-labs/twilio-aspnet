using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core;

public class ValidateTwilioRequestFilter : IEndpointFilter
{
    internal string AuthToken { get; set; }
    internal string BaseUrlOverride { get; set; }
    internal bool AllowLocal { get; set; }

    public ValidateTwilioRequestFilter(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetService<IOptions<TwilioRequestValidationOptions>>()?.Value;
        if (options == null) throw new Exception("RequestValidationOptions is not configured.");
        
        AuthToken = options.AuthToken;
        BaseUrlOverride = options.BaseUrlOverride?.TrimEnd('/');
        AllowLocal = options.AllowLocal ?? true;
    }

    public async ValueTask<object> InvokeAsync(
        EndpointFilterInvocationContext efiContext,
        EndpointFilterDelegate next
    )
    {
        var httpContext = efiContext.HttpContext;
        var request = httpContext.Request;
        string urlOverride = null;
        if (BaseUrlOverride != null)
        {
            urlOverride = $"{BaseUrlOverride}{request.Path}{request.QueryString}";
        }

        var validator = new RequestValidationHelper();

        if (validator.IsValidRequest(httpContext, AuthToken, urlOverride, AllowLocal))
        {
            return await next(efiContext);
        }

        return Results.StatusCode((int) HttpStatusCode.Forbidden);
    }
}

public static class TwilioFilterExtensions
{
    public static RouteHandlerBuilder ValidateTwilioRequest(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter<ValidateTwilioRequestFilter>();
    
    public static RouteGroupBuilder ValidateTwilioRequest(this RouteGroupBuilder builder)
        => builder.AddEndpointFilter<ValidateTwilioRequestFilter>();
}