using System.Web.Mvc;
using System.Xml.Linq;
using Twilio.TwiML;

namespace Twilio.AspNet.Mvc;

/// <summary>
/// Adds extension methods to the ControllerBase class for returning TwiML in MVC actions
/// </summary>
public static class TwilioControllerExtensions
{
    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static TwiMLResult TwiML(this ControllerBase controller, MessagingResponse response)
        => new(response);
        
    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="response"></param>
    /// <param name="formattingOptions"></param>
    /// <returns></returns>
    public static TwiMLResult TwiML(this ControllerBase controller, MessagingResponse response, SaveOptions formattingOptions)
        => new(response, formattingOptions);

    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static TwiMLResult TwiML(this ControllerBase controller, VoiceResponse response)
        => new(response);
        
    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="controller"></param>
    /// <param name="response"></param>
    /// <param name="formattingOptions"></param>
    /// <returns></returns>
    public static TwiMLResult TwiML(this ControllerBase controller, VoiceResponse response, SaveOptions formattingOptions)
        => new(response, formattingOptions);
}