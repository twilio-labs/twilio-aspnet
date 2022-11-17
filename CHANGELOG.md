## 7.0.0
New:
- The new `ValidateTwilioRequest` extension method and `ValidateTwilioRequestFilter` adds Twilio request validation to your endpoints and Minimal APIs, only for ASP.NET Core 7.
- The new `ValidateTwilioRequestMiddleware` adds Twilio request validation to the ASP.NET Core request pipeline. This is helpful for securing static files among other things that can't be secured using attributes and filters.
- New `TwiMLResult` constructor overloads to specify formatting of the `TwiML`. The `TwiML` extension methods and methods on `TwilioController` also have the new formatting overloads.
- `VoiceResponse` and `MessagingResponse` have a new extension method `ToTwiMLResult()` that will create a `TwiMLResult` instance for you.
- `SmsRequest` and `VoiceRequest` have been updated with parameters that were missing.
- Library now depends on version 6 of the Twilio C# library.

Breaking changes:
- You can no longer pass in a `string` or `XDocument` into the `TwiMLResult` constructor. Read the v7 announcement post for recommended alternatives.
- The public properties on `TwiMLResult` have been removed.
- The `HttpRequest.IsLocal()` extension method has been removed.
- The `Twilio.AspNet.Core.HttpStatusCodeResult` class has been removed in favor of the action results built into the framework.
- The `Twilio.AspNet.Core.MinimalApi` namespace has been removed. Types from the namespace have moved to the `Twilio.AspNet.Core` namespace.
- The `RequestValidationHelper` class is now static. You'll need to change your code to not instantiate this class and call its methods in a static manner.

Other changes include updated documentation with more samples and performance improvements.
Read about these changes in more detail at the [v7 announcement post](https://www.twilio.com/blog/whats-new-in-twilio-helper-library-for-aspnet-v7).

## 6.0.0 (2022-08-05)
- Big breaking change to the `[ValidateRequest]` attribute. The attribute no longer accepts parameters nor properties. Instead, you have to configure the request validation as documented in the readme.
- You can now add the Twilio REST client to ASP.NET Core's dependency injection container, using the `.AddTwilioClient` method. This Twilio client will use an `HttpClient` provided by an HTTP client factory. See readme for more details.
- We no longer try to match the Twilio SDK version number, and instead go by our own versioning to better communicate breaking changes vs minor changes.
- The projects are now built and packages are now pushed using GitHub Actions instead of AppVeyor.
- The projects are now built deterministically and support source link for better debugging.
- More samples have been added to the readme.

Read about these changes in more detail at the [v6 announcement post](https://www.twilio.com/blog/whats-new-in-twilio-helper-library-for-aspnet-v6). 

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
