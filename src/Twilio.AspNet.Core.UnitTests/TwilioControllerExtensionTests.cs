using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Moq;
using Twilio.TwiML;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class TwilioControllerExtensionTests
{

    [Fact]
    public async Task TwimlResult_Should_Write_VoiceResponse_To_ResponseBody()
    {
        var twiml = new VoiceResponse().Say("Hello World");
        var result = TwilioControllerExtensions.TwiML(Mock.Of<ControllerBase>(), twiml);
        var actionContext = CreateActionContext();
        await result.ExecuteResultAsync(actionContext);

        actionContext.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(actionContext.HttpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Equal(twiml.ToString(), responseBody);
    }

    [Fact]
    public async Task TwimlResult_Should_Write_MessagingResponse_To_ResponseBody()
    {
        var twiml = new MessagingResponse().Message("Hello World");
        var result = TwilioControllerExtensions.TwiML(Mock.Of<ControllerBase>(), twiml);
        var actionContext = CreateActionContext();
        await result.ExecuteResultAsync(actionContext);

        actionContext.HttpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(actionContext.HttpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Equal(twiml.ToString(), responseBody);
    }

    private ActionContext CreateActionContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        return new ActionContext(httpContext,
            new RouteData(),
            new ActionDescriptor(),
            new ModelStateDictionary());
    }
}