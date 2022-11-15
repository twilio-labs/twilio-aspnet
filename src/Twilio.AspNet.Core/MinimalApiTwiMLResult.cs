using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace Twilio.AspNet.Core;

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
public partial class TwiMLResult : IResult
{
    /// <summary>
    /// Writes the TwiML to the HTTP response body
    /// </summary>
    /// <param name="httpContext">The HttpContext containing the Response to write the TwiML to</param>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        return WriteTwiMLToResponse(httpContext.Response);
    }
}