using System;
using System.IO;
using System.Xml;
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
        public void TestXmlNodeTwiml()
        {
            var response = new XDocument(
                new XElement("Response",
                    new XElement("Say", "Ahoy!")
                )
            );

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
        public void TestStringTwiml()
        {
            var response = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                           "<Response>" +
                           "<Say>Ahoy!</Say>" +
                           "</Response>";

            var result = new TwiMLResult(response);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal(response, mocks.Response.Object.Output.ToString());
        }

        [Fact]
        public void TestNullTwiml()
        {
            var result = new TwiMLResult((TwiML.TwiML) null);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><Response></Response>",
                mocks.Response.Object.Output.ToString()
            );
            mocks.Response.Object.Output.Dispose();
            mocks.Response.Setup(r => r.Output).Returns(new Utf8StringWriter());
            
            result = new TwiMLResult((XDocument) null);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><Response></Response>",
                mocks.Response.Object.Output.ToString()
            );
            mocks.Response.Object.Output.Dispose();
            mocks.Response.Setup(r => r.Output).Returns(new Utf8StringWriter());
            
            result = new TwiMLResult((string) null);
            result.ExecuteResult(mocks.ControllerContext.Object);

            Assert.Equal(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?><Response></Response>",
                mocks.Response.Object.Output.ToString()
            );
            mocks.Response.Object.Output.Dispose();
        }
    }
}