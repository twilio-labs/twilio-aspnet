using System.Text;
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
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
		public TwiMLResult TwiML(MessagingResponse response)
		{
			return new TwiMLResult(response);
		}

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="encoding">Encoding to use for Xml</param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public TwiMLResult TwiML(MessagingResponse response, Encoding encoding)
        {
            return new TwiMLResult(response, encoding);
        }

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public TwiMLResult TwiML(VoiceResponse response)
        {
            return new TwiMLResult(response);
        }

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="response"></param>
        /// <param name="encoding">Encoding to use for Xml</param>
        /// <returns></returns>
        // ReSharper disable once InconsistentNaming
        public TwiMLResult TwiML(VoiceResponse response, Encoding encoding)
        {
            return new TwiMLResult(response, encoding);
        }
    }
}
