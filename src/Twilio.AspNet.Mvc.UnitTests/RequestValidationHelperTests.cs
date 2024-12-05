using System.Collections.Specialized;
using Xunit;

namespace Twilio.AspNet.Mvc.UnitTests;

public class RequestValidationHelperTests
{
    [Fact]
    public void TestLocal()
    {
        var fakeContext = new ContextMocks(true).HttpContext.Object;
        var result = RequestValidationHelper.IsValidRequest(fakeContext, "bad-token", true);

        Assert.True(result);
    }

    [Fact]
    public void TestNoLocal()
    {
        var fakeContext = new ContextMocks(true).HttpContext.Object;
        var result = RequestValidationHelper.IsValidRequest(fakeContext, "bad-token");

        Assert.False(result);
    }

    [Fact]
    public void TestNoForm()
    {
        var fakeContext = new ContextMocks(true).HttpContext.Object;
        var result = RequestValidationHelper.IsValidRequest(fakeContext, ContextMocks.FakeAuthToken);

        Assert.True(result);
    }

    [Fact]
    public void TestUrlOverrideFail()
    {
        var fakeContext = new ContextMocks(true).HttpContext.Object;
        var result = RequestValidationHelper.IsValidRequest(fakeContext, ContextMocks.FakeAuthToken, "https://example.com/");

        Assert.False(result);
    }


    [Fact]
    public void TestUrlOverride()
    {
        var fakeContext = new ContextMocks("https://example.com/", true).HttpContext.Object;
        var result = RequestValidationHelper.IsValidRequest(fakeContext, ContextMocks.FakeAuthToken, "https://example.com/");

        Assert.True(result);
    }

    [Fact]
    public void TestForm()
    {
        var form = new NameValueCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };
        var fakeContext = new ContextMocks(true, form).HttpContext.Object;
        var result = RequestValidationHelper.IsValidRequest(fakeContext, ContextMocks.FakeAuthToken);

        Assert.True(result);
    }
}