## 5.77.0 (2022-07-19)
- Twilio.AspNet.Core and Twilio.AspNet.Common now use .NET Standard 2.0 and dropped older .NET Standard versions.
- Microsoft.AspNetCore.Mvc.Core dependency has been updated to a version that is not vulnerable. For newer versions of .NET, a framework dependency is used instead.
- Twilio.AspNet.Mvc now targets .NET 4.6.2.
- Twilio.AspNet.Core and Twilio.AspNet.Mvc now depend on version 5.77.0 of the Twilio package.

## 5.71.0 (2022-04-11)
- Add extension methods to return `TwiML` without inheriting from `TwilioController` (https://github.com/twilio-labs/twilio-aspnet/pull/45)
- Fix Swagger bug in Twilio.AspNet.Core (https://github.com/twilio-labs/twilio-aspnet/pull/43)
- Update readme with more samples and updated language

## 5.71.0 (2022-02-11)
- Add support for returning `TwiML` in Minimal API using `Results.Extensions.TwiML` (https://github.com/twilio-labs/twilio-aspnet/pull/35)

## pre 5.71.0
- Draw some circles
- Draw the owl
- First release
- Broke out Twilio.AspNet.Common into separate package