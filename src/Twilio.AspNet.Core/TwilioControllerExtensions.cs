using Microsoft.AspNetCore.Mvc;
using Twilio.TwiML;

namespace Twilio.AspNet.Core
{
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
        // ReSharper disable once InconsistentNaming
        public static TwiMLResult TwiML(this ControllerBase controller, MessagingResponse response)
            => new TwiMLResult(response);

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="controller"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public static TwiMLResult TwiML(this ControllerBase controller, VoiceResponse response)
            => new TwiMLResult(response);
    }
}