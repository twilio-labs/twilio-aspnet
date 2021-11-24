using System;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class ContextMocks
    {
        public Moq.Mock<HttpContextBase> HttpContext { get; set; }
        public Moq.Mock<HttpRequestBase> Request { get; set; }

        public ContextMocks(bool isLocal, NameValueCollection form = null) : this("", isLocal, form)
        {
        }

        public ContextMocks(string urlOverride, bool isLocal, NameValueCollection form = null)
        {
            var headers = new NameValueCollection();
            headers.Add("X-Twilio-Signature", CalculateSignature(urlOverride, form));

            HttpContext = new Moq.Mock<HttpContextBase>();
            Request = new Moq.Mock<HttpRequestBase>();
            HttpContext.Setup(x => x.Request).Returns(Request.Object);
            Request.Setup(x => x.IsLocal).Returns(isLocal);
            Request.Setup(x => x.Headers).Returns(headers);
            Request.Setup(x => x.Url).Returns(new Uri(ContextMocks.fakeUrl));

            if (form != null)
            {
                Request.Setup(x => x.HttpMethod).Returns("POST");
                Request.Setup(x => x.Form).Returns(form);
            }
        }

        public static string fakeUrl = "https://api.example.com/webhook";
        public static string fakeAuthToken = "thisisafakeauthtoken";
        
        private string CalculateSignature(string urlOverride, NameValueCollection form)
        {
            var value = new StringBuilder();
            value.Append(string.IsNullOrEmpty(urlOverride) ? ContextMocks.fakeUrl : urlOverride);

            if (form != null)
            {
                var sortedKeys = form.AllKeys.OrderBy(k => k, StringComparer.Ordinal).ToList();
                foreach (var key in sortedKeys)
                {
                    value.Append(key);
                    value.Append(form[key]);
                }
            }

            var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(ContextMocks.fakeAuthToken));
            var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value.ToString()));

            return Convert.ToBase64String(hash);
        }
    }

    public class RequestValidationHelperTests
    {
        [Fact]
        public void TestLocal()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = (new ContextMocks(true)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, "bad-token", true);

            Assert.True(result);
        }

        [Fact]
        public void TestNoLocal()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = (new ContextMocks(true)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, "bad-token", false);

            Assert.False(result);
        }

        [Fact]
        public void TestNoForm()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = (new ContextMocks(true)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, false);

            Assert.True(result);
        }

        [Fact]
        public void TestUrlOverrideFail()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = (new ContextMocks(true)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, "https://example.com/", false);

            Assert.False(result);
        }


        [Fact]
        public void TestUrlOverride()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = (new ContextMocks("https://example.com/", true)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, "https://example.com/", false);

            Assert.True(result);
        }

        [Fact]
        public void TestForm()
        {
            var validator = new RequestValidationHelper();

            var form = new NameValueCollection();
            form.Add("key1", "value1");
            form.Add("key2", "value2");
            var fakeContext = (new ContextMocks(true, form)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, false);

            Assert.True(result);
        }
    }
}
