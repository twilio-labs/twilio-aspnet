using System.Web.Mvc;
using Twilio.TwiML;

namespace Twilio.AspNet.Mvc
{
    /// <summary>
    /// Extends the standard base controller to simplify returning a TwiML response
    /// </summary>
	public class TwilioController : Controller
	{
        /// <summary>
        /// Returns a property formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
		public TwiMLResult TwiML(MessagingResponse response)
		{
			return new TwiMLResult(response);
		}

        /// <summary>
        /// Returns a property formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public TwiMLResult TwiML(VoiceResponse response)
        {
            return new TwiMLResult(response);
        }
    }
}
