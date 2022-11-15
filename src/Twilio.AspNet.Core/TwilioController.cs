using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// Extends the standard base controller to simplify returning a TwiML response
    /// </summary>
	public class TwilioController : ControllerBase
    {
        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        [NonAction]
        public TwiMLResult TwiML(MessagingResponse response) => new TwiMLResult(response);
        
        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="formattingOptions"></param>
        /// <returns></returns>
        [NonAction]
        public TwiMLResult TwiML(MessagingResponse response, SaveOptions formattingOptions)
            => new TwiMLResult(response, formattingOptions);

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        [NonAction]
        public TwiMLResult TwiML(VoiceResponse response) => new TwiMLResult(response);

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="formattingOptions"></param>
        /// <returns></returns>
        [NonAction]
        public TwiMLResult TwiML(VoiceResponse response, SaveOptions formattingOptions)
            => new TwiMLResult(response, formattingOptions);
    }
}