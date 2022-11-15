using Twilio.TwiML;

namespace Twilio.AspNet.Core
{
    public static class TwiMLExtensions
    {
        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="voiceResponse"></param>
        /// <returns></returns>
        public static TwiMLResult ToTwiMLResult(this VoiceResponse voiceResponse) 
            => new TwiMLResult(voiceResponse);

        /// <summary>
        /// Returns a properly formatted TwiML response
        /// </summary>
        /// <param name="messagingResponse"></param>
        /// <returns></returns>
        public static TwiMLResult ToTwiMLResult(this MessagingResponse messagingResponse)
            =>new TwiMLResult(messagingResponse);
    }
}
