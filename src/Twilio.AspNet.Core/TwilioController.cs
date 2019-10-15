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
        // ReSharper disable once InconsistentNaming
        protected TwiMLResult TwiML(MessagingResponse response)
        {
            return new TwiMLResult(response);
        }

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        protected TwiMLResult TwiML(VoiceResponse response)
        {
            return new TwiMLResult(response);
        }
    }
}
