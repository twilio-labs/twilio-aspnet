using System.Threading.Tasks;
using System.Xml.Linq;
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
    public static TwiMLResult TwiML(this IResultExtensions results, TwiML.TwiML twimlResponse)
        => new(twimlResponse);
    
    /// <summary>
    /// Returns a TwiMLResult
    /// </summary>
    /// <param name="results">Results extensions interface</param>
    /// <param name="twimlResponse">The TwiML to write to the HTTP response body</param>
    /// <param name="formattingOptions">Specifies how to format TwiML</param>
    /// <returns>The TwiMLResult will write the TwiML to the HTTP response body</returns>
    public static TwiMLResult TwiML(
        this IResultExtensions results, 
        TwiML.TwiML twimlResponse, 
        SaveOptions formattingOptions
    ) => new(twimlResponse, formattingOptions);
}

/// <summary>
/// Writes TwiML object to the HTTP response body
/// </summary>
public partial class TwiMLResult : IResult
{
    /// <summary>
    /// Writes the TwiML to the HTTP response body
    /// </summary>
    /// <param name="httpContext">The HttpContext containing the Response to write the TwiML to</param>
    public Task ExecuteAsync(HttpContext httpContext) => WriteTwiMLToResponse(httpContext.Response);
}