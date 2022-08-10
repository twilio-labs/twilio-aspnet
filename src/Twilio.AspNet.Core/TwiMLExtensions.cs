using Twilio.TwiML;

namespace Twilio.AspNet.Core
{
    // ReSharper disable once InconsistentNaming
    public static class TwiMLExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static TwiMLResult ToTwiMLResult(this VoiceResponse voiceResponse)
        {
            return new TwiMLResult(voiceResponse);
        }

        // ReSharper disable once InconsistentNaming
        public static TwiMLResult ToTwiMLResult(this MessagingResponse messagingResponse)
        {
            return new TwiMLResult(messagingResponse);
        }
    }
}