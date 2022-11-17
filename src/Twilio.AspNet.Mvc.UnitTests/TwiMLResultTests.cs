using System;
using System.IO;
using System.Xml.Linq;
using Twilio.TwiML;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class TwiMLResultTest
    {
        private readonly ContextMocks mocks = new ContextMocks(true);
        private static readonly string NewLine = Environment.NewLine;

        [Fact]
        public void TestVoiceResponse()
        {
            var response = new VoiceResponse().Say("Ahoy!");

            var result = new TwiMLResult(response);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal($"<?xml version=\"1.0\" encoding=\"utf-8\"?>{NewLine}" +
                         $"<Response>{NewLine}" +
                         $"  <Say>Ahoy!</Say>{NewLine}" +
                         "</Response>",
                mocks.Response.Object.Output.ToString()
            );
        }

        [Fact]
        public void TestVoiceResponseUnformatted()
        {
            var response = new VoiceResponse().Say("Ahoy!");

            var result = new TwiMLResult(response, SaveOptions.DisableFormatting);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                         "<Response>" +
                         "<Say>Ahoy!</Say>" +
                         "</Response>",
                mocks.Response.Object.Output.ToString()
            );
        }

        [Fact]
        public void TestVoiceResponseUnformattedUtf16()
        {
            // string writer has Utf16 encoding
            mocks.Response.Setup(r => r.Output).Returns(new StringWriter());
            var response = new VoiceResponse().Say("Ahoy!");

            var result = new TwiMLResult(response, SaveOptions.DisableFormatting);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal("<?xml version=\"1.0\" encoding=\"utf-16\"?>" +
                         "<Response>" +
                         "<Say>Ahoy!</Say>" +
                         "</Response>",
                mocks.Response.Object.Output.ToString()
            );
        }

        [Fact]
        public void TestNullTwiml()
        {
            var result = new TwiMLResult(null);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><Response></Response>",
                mocks.Response.Object.Output.ToString()
            );
            mocks.Response.Object.Output.Dispose();
        }
    }
}