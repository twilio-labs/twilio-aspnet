using System.Xml.Linq;
using Twilio.TwiML;

namespace Twilio.AspNet.Core;

public static class TwiMLExtensions
{
    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="voiceResponse"></param>
    /// <returns></returns>
    public static TwiMLResult ToTwiMLResult(this VoiceResponse voiceResponse)
        => new(voiceResponse);
        
    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="voiceResponse"></param>
    /// <param name="formattingOptions">Specifies how to format TwiML</param>
    /// <returns></returns>
    public static TwiMLResult ToTwiMLResult(this VoiceResponse voiceResponse, SaveOptions formattingOptions)
        => new(voiceResponse, formattingOptions);

    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="messagingResponse"></param>
    /// <returns></returns>
    public static TwiMLResult ToTwiMLResult(this MessagingResponse messagingResponse)
        => new(messagingResponse);

    /// <summary>
    /// Returns a properly formatted TwiML response
    /// </summary>
    /// <param name="messagingResponse"></param>
    /// <param name="formattingOptions">Specifies how to format TwiML</param>
    /// <returns></returns>
    public static TwiMLResult ToTwiMLResult(this MessagingResponse messagingResponse, SaveOptions formattingOptions)
        => new(messagingResponse, formattingOptions);
}