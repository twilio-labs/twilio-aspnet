using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Twilio.AspNet.Core;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class ContextMocks
    {
        public Moq.Mock<HttpContext> HttpContext { get; set; }
        public Moq.Mock<HttpRequest> Request { get; set; }

        public ContextMocks(bool isLocal, FormCollection? form = null, bool isProxied = false) : this("", isLocal, form, isProxied)
        {
        }

        public ContextMocks(string urlOverride, bool isLocal, FormCollection? form = null, bool isProxied = false)
        {
            var headers = new HeaderDictionary();
            headers.Add("X-Twilio-Signature", CalculateSignature(urlOverride, form));
            if (isProxied)
            {
                headers.Add("X-Forwarded-For", "1.1.1.1");
            }

            var connectionInfo = new Moq.Mock<ConnectionInfo>();
            connectionInfo.Setup(x => x.RemoteIpAddress).Returns(isLocal ? IPAddress.Loopback : IPAddress.Parse("1.1.1.1"));

            HttpContext = new Moq.Mock<HttpContext>();
            Request = new Moq.Mock<HttpRequest>();
            HttpContext.Setup(x => x.Request).Returns(Request.Object);
            HttpContext.Setup(x => x.Connection).Returns(connectionInfo.Object);
            Request.Setup(x => x.Headers).Returns(headers);
            Request.Setup(x => x.HttpContext).Returns(HttpContext.Object);

            var uri = new Uri(ContextMocks.fakeUrl);
            Request.Setup(x => x.QueryString).Returns(new QueryString(uri.Query));
            Request.Setup(x => x.Scheme).Returns(uri.Scheme);
            Request.Setup(x => x.Host).Returns(new HostString(uri.Host));
            Request.Setup(x => x.Path).Returns(new PathString(uri.AbsolutePath));

            if (form != null)
            {
                Request.Setup(x => x.Method).Returns("POST");
                Request.Setup(x => x.Form).Returns(form);
            }
        }

        public static string fakeUrl = "https://api.example.com/webhook";
        public static string fakeAuthToken = "thisisafakeauthtoken";
        
        private string CalculateSignature(string urlOverride, FormCollection? form)
        {
            var value = new StringBuilder();
            value.Append(string.IsNullOrEmpty(urlOverride) ? ContextMocks.fakeUrl : urlOverride);

            if (form != null)
            {
                var sortedKeys = form.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
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
        public void TestNoLocalDueToProxy()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = (new ContextMocks(true, isProxied: true)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, "bad-token", true);

            Assert.False(result);
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
        public void TestBadForm()
        {
            var validator = new RequestValidationHelper();

            var contextMocks = new ContextMocks(true);
            var fakeContext = contextMocks.HttpContext.Object;
            contextMocks.Request.Setup(x => x.Method).Returns("POST");
            contextMocks.Request.Setup(x => x.Form).Throws(new Exception("poof!"));

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

            var form = new FormCollection(new Dictionary<string, StringValues>() {
                {"key1", "value1"},
                {"key2", "value2"}
            });
            var fakeContext = (new ContextMocks(true, form)).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, false);

            Assert.True(result);
        }
    }
}
