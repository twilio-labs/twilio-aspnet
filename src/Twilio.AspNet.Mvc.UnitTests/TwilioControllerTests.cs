using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class TwilioControllerTests
    {
        const string ASCII_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string UNICODE_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äåéö";

        [Fact]
        public void TestVoiceResponseDefaultEncodingPass()
        {
            var response = this.GetVoiceResponse(ASCII_CHARS);

            var result = new TwilioController().TwiML(response);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseDefaultEncodingFail()
        {
            var response = this.GetVoiceResponse(UNICODE_CHARS);

            Assert.ThrowsAny<Exception>(() =>
            {
                var result = new TwilioController().TwiML(response);
            });
        }

        [Fact]
        public void TestVoiceResponseUnicodeEncodingASCII()
        {
            var response = this.GetVoiceResponse(ASCII_CHARS);

            var result = new TwilioController().TwiML(response, Encoding.UTF8);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseUnicodeEncodingUTF8()
        {
            var response = this.GetVoiceResponse(UNICODE_CHARS);

            var result = new TwilioController().TwiML(response, Encoding.UTF8);

            Assert.Contains(UNICODE_CHARS, result.Data.ToString());
        }

        //messaging
        [Fact]
        public void TestMessagingResponseDefaultEncodingPass()
        {
            var response = this.GetMessagingResponse(ASCII_CHARS);

            var result = new TwilioController().TwiML(response);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestMessagingResponseDefaultEncodingFail()
        {
            var response = this.GetMessagingResponse(UNICODE_CHARS);

            Assert.ThrowsAny<Exception>(() =>
            {
                var result = new TwilioController().TwiML(response);
            });
        }

        [Fact]
        public void TestMessagingResponseUnicodeEncodingASCII()
        {
            var response = this.GetMessagingResponse(ASCII_CHARS);

            var result = new TwilioController().TwiML(response, Encoding.UTF8);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestMessagingResponsUnicodeEncodingUTF8()
        {
            var response = this.GetVoiceResponse(UNICODE_CHARS);

            var result = new TwilioController().TwiML(response, Encoding.UTF8);

            Assert.Contains(UNICODE_CHARS, result.Data.ToString());
        }

        private TwiML.VoiceResponse GetVoiceResponse(string content)
        {
            return new TwiML.VoiceResponse().Say(content);
        }

        private TwiML.MessagingResponse GetMessagingResponse(string content)
        {
            return new TwiML.MessagingResponse().Message(content);
        }
    }
}
