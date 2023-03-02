using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

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
        if (RequestValidationHelper.IsValidRequest(efiContext.HttpContext))
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