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
            Data = LoadFromString(twiml, Encoding.Default);
        }

        public TwiMLResult(string twiml, Encoding encoding)
        {
            Data = LoadFromString(twiml, encoding);
        }

        public TwiMLResult(XDocument twiml)
        {
            Data = twiml;
        }

        public TwiMLResult(MessagingResponse response)
        {
            if (response != null)
                Data = LoadFromString(response.ToString(), Encoding.Default);
        }

        public TwiMLResult(MessagingResponse response, Encoding encoding)
        {
            if (response != null)
                Data = LoadFromString(response.ToString(), encoding);
        }

        public TwiMLResult(VoiceResponse response)
        {
            if (response != null)
                Data = LoadFromString(response.ToString(), Encoding.Default);
        }

        public TwiMLResult(VoiceResponse response, Encoding encoding)
        {
            if (response != null)
                Data = LoadFromString(response.ToString(), encoding);
        }

        private static XDocument LoadFromString(string twiml, Encoding encoding)
        {
            var stream = new MemoryStream(encoding.GetBytes(twiml));

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
