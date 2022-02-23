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
    public async Task TestTwimlResultWritesToResponseBody()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();

        var twiml = GetVoiceResponse();
        var twimlResult = Results.Extensions.TwiML(twiml);
        await twimlResult.ExecuteAsync(httpContext);

        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(httpContext.Response.Body);
        var responseBody = await reader.ReadToEndAsync();
        Assert.Equal(twiml.ToString(), responseBody);
    }

    private static VoiceResponse GetVoiceResponse() => new TwiML.VoiceResponse().Say("Hello World");
}