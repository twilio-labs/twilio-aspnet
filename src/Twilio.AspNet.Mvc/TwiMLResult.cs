using System.Web.Mvc;
using System.Xml.Linq;

namespace Twilio.AspNet.Mvc
{
    public class TwiMLResult : ActionResult
    {
        private readonly string dataString;
        private readonly XDocument dataDocument;
        private readonly SaveOptions formattingOptions;
        private readonly TwiML.TwiML dataTwiml;

        public TwiMLResult()
        {
        }

        public TwiMLResult(string twiml)
        {
            this.dataString = twiml;
        }

        public TwiMLResult(XDocument twiml) : this(twiml, SaveOptions.None)
        {
        }

        public TwiMLResult(XDocument twiml, SaveOptions formattingOptions)
        {
            this.dataDocument = twiml;
            this.formattingOptions = formattingOptions;
        }

        public TwiMLResult(TwiML.TwiML response) : this(response, SaveOptions.None)
        {
        }

        public TwiMLResult(TwiML.TwiML response, SaveOptions formattingOptions)
        {
            this.dataTwiml = response;
            this.formattingOptions = formattingOptions;
        }

        public override void ExecuteResult(ControllerContext controllerContext)
        {
            var response = controllerContext.HttpContext.Response;
            response.ContentType = "application/xml";

            if (!string.IsNullOrEmpty(dataString))
            {
                response.Output.Write(dataString);
                return;
            }
            
            if (dataDocument != null)
            {
                dataDocument.Save(response.Output, formattingOptions);
                return;
            }
            
            if (dataTwiml != null)
            {
                response.Output.Write(dataTwiml.ToString(formattingOptions));
                return;
            }
            
            response.Output.Write("<Response></Response>");
        }
    }
}
