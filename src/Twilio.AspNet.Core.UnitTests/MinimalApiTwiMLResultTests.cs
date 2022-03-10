using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Twilio.AspNet.Core.MinimalApi;
using Twilio.TwiML;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

// ReSharper disable once InconsistentNaming
public class MinimalApiTwiMLResultTests
{
    [Fact]
    public Task TwimlResult_Should_Write_VoiceResponse_To_ResponseBody() =>
        ValidateTwimlResultWritesToResponseBody(GetVoiceResponse());

    [Fact]
    public Task TwimlResult_Should_Write_MessagingResponse_To_ResponseBody() =>
        ValidateTwimlResultWritesToResponseBody(GetMessagingResponse());
    
    private static async Task ValidateTwimlResultWritesToResponseBody(TwiML.TwiML twiMlResponse)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var twimlResult = Results.Extensions.TwiML(twiMlResponse);
        await twimlResult.ExecuteAsync(httpContext);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Equal(twiMlResponse.ToString(), responseBody);
    }

    private static VoiceResponse GetVoiceResponse() => new VoiceResponse().Say("Hello World");
    private static MessagingResponse GetMessagingResponse() => new MessagingResponse().Message("Hello World");
}