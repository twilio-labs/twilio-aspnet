using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Twilio.TwiML;

// ReSharper disable once CheckNamespace
namespace Twilio.AspNet.Core.MinimalApi;

/// <summary>
/// Adds extension methods to Results.Extensions to write TwiML objects to the HTTP response body
/// </summary>
public static class ResultsExtensions
{
    /// <summary>
    /// Returns a TwiMLResult
    /// </summary>
    /// <param name="results">Results extensions interface</param>
    /// <param name="twimlResponse">The TwiML to write to the HTTP response body</param>
    /// <returns>The TwiMLResult will write the TwiML to the HTTP response body</returns>
    // ReSharper disable once InconsistentNaming
    // ReSharper disable once UnusedParameter.Global
    public static TwiMLResult TwiML(this IResultExtensions results, TwiML.TwiML twimlResponse)
        => new TwiMLResult(twimlResponse);
}

/// <summary>
/// Writes TwiML object to the HTTP response body
/// </summary>
// ReSharper disable once InconsistentNaming
public class TwiMLResult : IResult
{
    // ReSharper disable once InconsistentNaming
    private string twiML;

    /// <summary>
    /// Creates a TwiMLResult object
    /// </summary>
    /// <param name="twimlResponse">The TwiML to write to the HTTP response body</param>
    // ReSharper disable once InconsistentNaming
    public TwiMLResult(TwiML.TwiML twimlResponse)
    {
        twiML = twimlResponse?.ToString();
    }

    /// <summary>
    /// Writes the TwiML to the HTTP response body
    /// </summary>
    /// <param name="httpContext">The HttpContext containing the Response to write the TwiML to</param>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        twiML ??= "<Response></Response>";

        httpContext.Response.ContentType = "application/xml";
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(twiML);
        return httpContext.Response.WriteAsync(twiML);
    }
}

// ReSharper disable once InconsistentNaming
public static class TwiMLExtensions
{
    // ReSharper disable once InconsistentNaming
    public static TwiMLResult ToTwiMLResult(this VoiceResponse voiceResponse)
    {
        return new TwiMLResult(voiceResponse);
    }

    // ReSharper disable once InconsistentNaming
    public static TwiMLResult ToTwiMLResult(this MessagingResponse messagingResponse)
    {
        return new TwiMLResult(messagingResponse);
    }
}