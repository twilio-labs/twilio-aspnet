using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace dotnet6.Controllers;

public class SmsController : TwilioController
{
    [ValidateRequest("your auth token here", urlOverride: "https://??????.ngrok.io/sms")]
    public TwiMLResult Index()
    {
        var messagingResponse = new MessagingResponse();
        messagingResponse.Message("The Robots are coming! Head for the hills!!");

        return TwiML(messagingResponse);
    }
}
