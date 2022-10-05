using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Twilio.TwiML;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class ContextMocks
    {
        public Moq.Mock<HttpContextBase> HttpContext { get; set; }
        public Moq.Mock<HttpRequestBase> Request { get; set; }
        public Moq.Mock<HttpResponseBase> Response { get; set; }
        public Moq.Mock<ControllerContext> ControllerContext { get; set; }

        public ContextMocks(bool isLocal, NameValueCollection form = null) : this("", isLocal, form)
        {
        }

        public ContextMocks(string urlOverride, bool isLocal, NameValueCollection form = null)
        {
            var headers = new NameValueCollection();
            headers.Add("X-Twilio-Signature", CalculateSignature(urlOverride, form));

            HttpContext = new Moq.Mock<HttpContextBase>();
            Request = new Moq.Mock<HttpRequestBase>();
            Response = new Moq.Mock<HttpResponseBase>();
            ControllerContext = new Moq.Mock<ControllerContext>();
            
            HttpContext.Setup(x => x.Request).Returns(Request.Object);
            HttpContext.Setup(x => x.Response).Returns(Response.Object);
            ControllerContext.Setup(x => x.HttpContext).Returns(HttpContext.Object);
            
            Request.Setup(x => x.IsLocal).Returns(isLocal);
            Request.Setup(x => x.Headers).Returns(headers);
            Request.Setup(x => x.Url).Returns(new Uri(ContextMocks.fakeUrl));
            if (form != null)
            {
                Request.Setup(x => x.HttpMethod).Returns("POST");
                Request.Setup(x => x.Form).Returns(form);
            }
            
            Response.Setup(x => x.Output).Returns(new Utf8StringWriter());
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
}