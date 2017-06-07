using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class TwiMLResultTests
    {
        const string ASCII_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        const string UNICODE_CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äåéö";        

        //string constructor

        [Fact]
        public void TestStringDefaultEncodingPass()
        {
            var responseString = this.GetVoiceResponse(ASCII_CHARS).ToString();

            var result = new TwiMLResult(responseString);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestStringDefaultEncodingFail()
        {
            var responseString = this.GetVoiceResponse(UNICODE_CHARS).ToString();

            Assert.ThrowsAny<Exception>(() =>
            {
                var result = new TwiMLResult(responseString);
            });            
        }

        [Fact]
        public void TestStringUnicodeEncodingASCII()
        {
            var responseString = this.GetVoiceResponse(ASCII_CHARS).ToString();

            var result = new TwiMLResult(responseString, Encoding.UTF8);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestStringUnicodeEncodingUTF8()
        {
            var responseString = this.GetVoiceResponse(UNICODE_CHARS).ToString();

            var result = new TwiMLResult(responseString, Encoding.UTF8);

            Assert.Contains(UNICODE_CHARS, result.Data.ToString());
        }

        //voice response constructor
        [Fact]
        public void TestVoiceResponseDefaultEncodingPass()
        {
            var response = this.GetVoiceResponse(ASCII_CHARS);

            var result = new TwiMLResult(response);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseDefaultEncodingFail()
        {
            var response = this.GetVoiceResponse(UNICODE_CHARS);

            Assert.ThrowsAny<Exception>(() =>
            {
                var result = new TwiMLResult(response);
            });
        }

        [Fact]
        public void TestVoiceResponseUnicodeEncodingASCII()
        {
            var response = this.GetVoiceResponse(ASCII_CHARS);

            var result = new TwiMLResult(response, Encoding.UTF8);

            Assert.Contains(ASCII_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseUnicodeEncodingUTF8()
        {
            var response = this.GetVoiceResponse(UNICODE_CHARS);

            var result = new TwiMLResult(response, Encoding.UTF8);

            Assert.Contains(UNICODE_CHARS, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseExplicitDefaultEncodingFail()
        {
            var response = this.GetVoiceResponse(UNICODE_CHARS);

            Assert.ThrowsAny<Exception>(() =>
            {
                var result = new TwiMLResult(response, Encoding.Default);
            });
        }

        private TwiML.VoiceResponse GetVoiceResponse(string content)
        {
            return new TwiML.VoiceResponse().Say(content);
        }
    }
}
