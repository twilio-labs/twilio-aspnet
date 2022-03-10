using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace dotnet6.Controllers;

[ApiController]
[Route("[controller]")]
public class SmsController : TwilioController
{
    [ValidateRequest("your auth token here", urlOverride: "https://??????.ngrok.io/sms")]
    [HttpGet, HttpPost]
    public TwiMLResult Index()
    {
        var messagingResponse = new MessagingResponse();
        messagingResponse.Message("The Robots are coming! Head for the hills!!");

        return TwiML(messagingResponse);
    }
}
