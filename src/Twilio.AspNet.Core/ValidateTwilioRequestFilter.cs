using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core;

/// <summary>
/// Validates that incoming HTTP request originates from Twilio.
/// </summary>
public class ValidateTwilioRequestFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(
        EndpointFilterInvocationContext efiContext,
        EndpointFilterDelegate next
    )
    {
        var context = efiContext.HttpContext;
        var options = context.RequestServices
            .GetRequiredService<IOptionsSnapshot<TwilioRequestValidationOptions>>().Value;
            
        var authToken = options.AuthToken;
        var baseUrlOverride = options.BaseUrlOverride;
        var allowLocal = options.AllowLocal ?? true;
        
        var request = context.Request;
        
        string urlOverride = null;
        if (!string.IsNullOrEmpty(baseUrlOverride))
        {
            urlOverride = $"{baseUrlOverride}{request.Path}{request.QueryString}";
        }

        if (RequestValidationHelper.IsValidRequest(context, authToken, urlOverride, allowLocal))
        {
            return await next(efiContext);
        }

        return Results.StatusCode((int) HttpStatusCode.Forbidden);
    }
}

public static class TwilioFilterExtensions
{
    /// <summary>
    /// Validates that incoming HTTP request originates from Twilio.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static RouteHandlerBuilder ValidateTwilioRequest(this RouteHandlerBuilder builder)
        => builder.AddEndpointFilter<ValidateTwilioRequestFilter>();
    
    /// <summary>
    /// Validates that incoming HTTP request originates from Twilio.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static RouteGroupBuilder ValidateTwilioRequest(this RouteGroupBuilder builder)
        => builder.AddEndpointFilter<ValidateTwilioRequestFilter>();
}