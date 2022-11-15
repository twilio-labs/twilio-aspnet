using System.Collections.Specialized;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests
{
    public class RequestValidationHelperTests
    {
        [Fact]
        public void TestLocal()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = new ContextMocks(true).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, "bad-token", true);

            Assert.True(result);
        }

        [Fact]
        public void TestNoLocal()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = new ContextMocks(true).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, "bad-token", false);

            Assert.False(result);
        }

        [Fact]
        public void TestNoForm()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = new ContextMocks(true).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, false);

            Assert.True(result);
        }

        [Fact]
        public void TestUrlOverrideFail()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = new ContextMocks(true).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, "https://example.com/", false);

            Assert.False(result);
        }


        [Fact]
        public void TestUrlOverride()
        {
            var validator = new RequestValidationHelper();

            var fakeContext = new ContextMocks("https://example.com/", true).HttpContext.Object;
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
            var fakeContext = new ContextMocks(true, form).HttpContext.Object;
            var result = validator.IsValidRequest(fakeContext, ContextMocks.fakeAuthToken, false);

            Assert.True(result);
        }
    }
}
