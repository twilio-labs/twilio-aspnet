using System.Text;
using System.Xml;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    // ReSharper disable once InconsistentNaming
    public class TwiMLResultTests
    {
        public const string AsciiChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        public const string UnicodeChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890äåéö";        

        //string constructor
        [Fact]
        public void TestStringDefaultEncodingPass()
        {
            var responseString = GetVoiceResponse(UnicodeChars).ToString();

            var result = new TwiMLResult(responseString);

            Assert.Contains(UnicodeChars, result.Data.ToString());
        }

        [Fact]
        public void TestStringAsciiEncodingFail()
        {
            var responseString = GetVoiceResponse(UnicodeChars).ToString();

            var result = new TwiMLResult(responseString, Encoding.ASCII);

            Assert.Contains(AsciiChars, result.Data.ToString());
            Assert.DoesNotContain(UnicodeChars, result.Data.ToString());
        }

        [Fact]
        public void TestStringUnicodeEncodingUtf8()
        {
            var responseString = GetVoiceResponse(UnicodeChars).ToString();

            var result = new TwiMLResult(responseString, Encoding.UTF8);

            Assert.Contains(UnicodeChars, result.Data.ToString());
        }

        //voice response constructor
        [Fact]
        public void TestVoiceResponseDefaultEncodingPass()
        {
            var response = GetVoiceResponse(UnicodeChars);

            var result = new TwiMLResult(response);

            Assert.Contains(UnicodeChars, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseAsciiEncodingFail()
        {
            var response = GetVoiceResponse(UnicodeChars);

            var result = new TwiMLResult(response, Encoding.ASCII);

            Assert.Contains(AsciiChars, result.Data.ToString());
            Assert.DoesNotContain(UnicodeChars, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseUnicodeEncodingUtf8()
        {
            var response = GetVoiceResponse(UnicodeChars);

            var result = new TwiMLResult(response, Encoding.UTF8);

            Assert.Contains(UnicodeChars, result.Data.ToString());
        }

        [Fact]
        public void TestVoiceResponseExplicitDefaultEncodingFail()
        {
            var response = GetVoiceResponse(UnicodeChars);

            Assert.Throws<XmlException>(() =>
            {
                new TwiMLResult(response, Encoding.Default);
            });
        }

        public TwiML.VoiceResponse GetVoiceResponse(string content)
        {
            return new TwiML.VoiceResponse().Say(content);
        }
    }
}
