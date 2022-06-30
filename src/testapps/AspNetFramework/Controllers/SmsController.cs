using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace AspNetFramework.Controllers
{
    public class SmsController : TwilioController
    {
        [ValidateRequest]
        public TwiMLResult Index()
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message("The Robots are coming! Head for the hills!!");

            return TwiML(messagingResponse);
        }
    }
}
