using System.Threading;
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
        private readonly TwiML.TwiML twiml;
        private readonly SaveOptions formattingOptions;

        /// <param name="twiml">The TwiML to respond with</param>
        public TwiMLResult(TwiML.TwiML twiml) : this(twiml, SaveOptions.None)
        {
        }

        /// <param name="twiml">The TwiML to respond with</param>
        /// <param name="formattingOptions">Specifies how to format TwiML</param>
        public TwiMLResult(TwiML.TwiML twiml, SaveOptions formattingOptions)
        {
            this.twiml = twiml;
            this.formattingOptions = formattingOptions;
        }

        public async Task ExecuteResultAsync(ActionContext actionContext)
        {
            var response = actionContext.HttpContext.Response;
            await WriteTwiMLToResponse(response, actionContext.HttpContext.RequestAborted)
                .ConfigureAwait(false);
        }

        private async Task WriteTwiMLToResponse(HttpResponse response, CancellationToken cancellationToken)
        {
            response.ContentType = "application/xml";
            if (twiml == null)
            {
                await response.WriteAsync("<Response></Response>", cancellationToken)
                    .ConfigureAwait(false);
                return;
            }

            var doc = twiml.ToXDocument();

#if NET5_0_OR_GREATER
            await doc.SaveAsync(response.Body, formattingOptions, cancellationToken)
                .ConfigureAwait(false);
#else
            await response.WriteAsync(doc.ToString(formattingOptions), cancellationToken)
                .ConfigureAwait(false);
#endif
        }
    }
}