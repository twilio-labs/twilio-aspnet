# Twilio.AspNet

Twilio tools for ASP.NET MVC 3+ and eventually Web API and Core.

## Incoming SMS

```c#
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace WebApplication23.Controllers
{
    public class SmsController : TwilioController
    {
        // GET: Sms
        public TwiMLResult Index(SmsRequest request)
        {
            var response = new MessagingResponse();
            response.Message(
                $"Hey there {request.From}! " +
                "How 'bout those Seahawks?"
            );
            return TwiML(response);
        }
    }
}
```

## Incoming Voice Call

```c#
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace WebApplication23.Controllers
{
    public class VoiceController : TwilioController
    {
        // GET: Voice
        public TwiMLResult Index(VoiceRequest request)
        {
            var response = new VoiceResponse();
            response.Say($"Welcome. Are you from {request.FromCity}?");
            return TwiML(response);
        }
    }
}
```
