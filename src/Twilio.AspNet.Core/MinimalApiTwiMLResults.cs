using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Twilio.TwiML;

namespace Twilio.AspNet.Core.MinimalApi
{
    public static class ResultsExtensions
    {
        public static IResult TwiML(this IResultExtensions resultExtensions, MessagingResponse response)
            => new TwiMLResult(response);

        public static IResult TwiML(this IResultExtensions resultExtensions, VoiceResponse response)
            => new TwiMLResult(response);
    }

    public class TwiMLResult : IResult
    {
        private string twiML;

        public TwiMLResult(MessagingResponse response)
        {
            twiML = response?.ToString();
        }

        public TwiMLResult(VoiceResponse response)
        {
            twiML = response?.ToString();
        }

        public Task ExecuteAsync(HttpContext httpContext)
        {
            twiML ??= "<Response></Response>";

            httpContext.Response.ContentType = "application/xml";
            httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(twiML);
            return httpContext.Response.WriteAsync(twiML);
        }
    }
}