# Twilio.AspNet

[![Build status](https://ci.appveyor.com/api/projects/status/813hnjynh8ncamwj?svg=true)](https://ci.appveyor.com/project/TwilioAPI/twilio-aspnet)

ASP.NET tools for use with v5.x of the [Twilio helper library](https://github.com/twilio/twilio-csharp) for use with:
- ASP.NET MVC 3-5 on the .NET Framework
- ASP.NET Core 1-2 on .NET Core

You only need this library if you wish to respond to Twilio webhooks for
voice calls and SMS messages. If you only need to use the Twilio REST API's,
then you only need the [Twilio helper library](https://github.com/twilio/twilio-csharp).

## Twilio.AspNet.Mvc

### Requirements

Requires .NET 4.5.1 or later with ASP.NET MVC 3-5.

### Installation

```
Install-Package Twilio.AspNet.Mvc
```

## Twilio.AspNet.Core

### Requirements

Requires .NET Core 1.0 or later with ASP.NET Core 1.0 or later.

### Installation

```
Install-Package Twilio.AspNet.Core
```

## Code Samples for Either Library

### Incoming SMS

```c#
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc; // or .Core
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

### Incoming Voice Call

```c#
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc; // or .Core
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
