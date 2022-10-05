using System.Xml.Linq;
using Twilio.TwiML;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class TwilioControllerTests
    {
        private readonly ContextMocks mocks = new ContextMocks(true);

        [Fact]
        public void TestVoiceResponse()
        {
            var response = new VoiceResponse().Say("Ahoy!");

            var result = new TwilioController().TwiML(response);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                         "<Response>\n" +
                         "  <Say>Ahoy!</Say>\n" +
                         "</Response>",
                mocks.Response.Object.Output.ToString()
            );
        }
        
        [Fact]
        public void TestVoiceResponseUnformatted()
        {
            var response = new VoiceResponse().Say("Ahoy!");

            var result = new TwilioController().TwiML(response, SaveOptions.DisableFormatting);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                         "<Response>" +
                         "<Say>Ahoy!</Say>" +
                         "</Response>",
                mocks.Response.Object.Output.ToString()
            );
        }
    }
}