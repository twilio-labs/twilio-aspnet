using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Twilio.AspNet.Core
{
    /// <summary>
    /// TwiMLResult writes TwiML to the HTTP response body
    /// </summary>
    public partial class TwiMLResult : IActionResult
    {
        public string Data { get; protected set; }

        public TwiMLResult()
        {
        }

        public TwiMLResult(string twiml)
        {
            Data = twiml;
        }

        public TwiMLResult(TwiML.TwiML response) : this(response, SaveOptions.None)
        {
        }
        
        public TwiMLResult(TwiML.TwiML response, SaveOptions formattingOptions)
        {
            if (response != null)
                Data = response.ToString(formattingOptions);
        }

        public async Task ExecuteResultAsync(ActionContext actionContext)
        {
            var response = actionContext.HttpContext.Response;
            await WriteTwiMLToResponse(response);
        }
        
        private async Task WriteTwiMLToResponse(HttpResponse response)
        {
            response.ContentType = "application/xml";
            if (Data == null)
            {
                Data = "<Response></Response>";
            }

            await response.WriteAsync(Data);
        }
    }
}
