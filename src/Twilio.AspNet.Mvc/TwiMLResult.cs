using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Twilio.TwiML;

namespace Twilio.AspNet.Mvc
{
    // ReSharper disable once InconsistentNaming
    public class TwiMLResult : ActionResult
    {
        public XDocument Data { get; protected set; }

        public TwiMLResult()
        {
        }

        public TwiMLResult(string twiml)
        {
            Data = LoadFromString(twiml);
        }

        public TwiMLResult(XDocument twiml)
        {
            Data = twiml;
        }

        public TwiMLResult(MessagingResponse response)
        {
            if (response != null)
                Data = LoadFromString(response.ToString());
        }

        public TwiMLResult(VoiceResponse response)
        {
            if (response != null)
                Data = LoadFromString(response.ToString());
        }

        private static XDocument LoadFromString(string twiml)
        {
            var stream = new MemoryStream(Encoding.Default.GetBytes(twiml));

            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Prohibit;

            var reader = XmlReader.Create(stream, settings);
            return XDocument.Load(reader);
        }

        public override void ExecuteResult(ControllerContext controllerContext)
        {
            var context = controllerContext.RequestContext.HttpContext;
            context.Response.ContentType = "application/xml";

            if (Data == null)
            {
                Data = new XDocument(new XElement("Response"));
            }

            Data.Save(context.Response.Output);
        }
    }
}
