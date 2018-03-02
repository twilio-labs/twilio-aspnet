using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class TwilioControllerTests
    {
        [Fact]
        public void TestVoiceResponseDefaultEncodingPass()
        {
            var response = GetVoiceResponse(TwiMLResultTests.UnicodeChars);

            var result = new TwilioController().TwiML(response);

            Assert.Contains(TwiMLResultTests.UnicodeChars, result.Data.ToString());
        }

        [Fact]
        public void TestMessagingResponseDefaultEncodingPass()
        {
            var response = GetMessagingResponse(TwiMLResultTests.UnicodeChars);

            var result = new TwilioController().TwiML(response);

            Assert.Contains(TwiMLResultTests.UnicodeChars, result.Data.ToString());
        }

        private static TwiML.VoiceResponse GetVoiceResponse(string content)
        {
            return new TwiML.VoiceResponse().Say(content);
        }

        private static TwiML.MessagingResponse GetMessagingResponse(string content)
        {
            return new TwiML.MessagingResponse().Message(content);
        }
    }
}
