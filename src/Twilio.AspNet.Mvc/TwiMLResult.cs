using System.Web.Mvc;
using System.Xml.Linq;

namespace Twilio.AspNet.Mvc;

public class TwiMLResult : ActionResult
{
    private readonly SaveOptions _formattingOptions;
    private readonly TwiML.TwiML _dataTwiml;

    public TwiMLResult(TwiML.TwiML response) : this(response, SaveOptions.None)
    {
    }

    public TwiMLResult(TwiML.TwiML response, SaveOptions formattingOptions)
    {
        _dataTwiml = response;
        _formattingOptions = formattingOptions;
    }

    public override void ExecuteResult(ControllerContext controllerContext)
    {
        var response = controllerContext.HttpContext.Response;
        var encoding = response.Output.Encoding.BodyName;
        response.ContentType = "application/xml";

        if (_dataTwiml is null)
        {
            response.Output.Write($"<?xml version=\"1.0\" encoding=\"{encoding}\"?><Response></Response>");
            return;
        }

        var twimlString = _dataTwiml.ToString(_formattingOptions);
        if (encoding != "utf-8")
        {
            twimlString = twimlString.Replace("utf-8", encoding);
        }

        response.Output.Write(twimlString);
    }
}