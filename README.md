# Twilio helper library for ASP.NET

[![Build](https://github.com/twilio-labs/twilio-aspnet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/twilio-labs/twilio-aspnet/actions/workflows/ci.yml)

**The Twilio helper library for ASP.NET (Twilio.AspNet), helps you integrate the official [Twilio SDK for C# and .NET](https://github.com/twilio/twilio-csharp) into your ASP.NET applications.** The library supports ASP.NET MVC on .NET Framework and ASP.NET Core.

This library helps you respond to webhooks, adds the Twilio client to the dependency injection container, and validate HTTP request originate from Twilio.

## Twilio.AspNet.Core
[![NuGet Badge](https://buildstats.info/nuget/Twilio.AspNet.Core)](https://www.nuget.org/packages/Twilio.AspNet.Core/) 
### Requirements

Requires .NET (Core) 2.0 or later.

### Installation
Run the following command to install the package using the .NET CLI:
```bash
dotnet add package Twilio.AspNet.Core
```

Alternatively, from the Package Manager Console or Developer PowerShell, run the following command to install the latest version: 
```PowerShell
Install-Package Twilio.AspNet.Core
```
Alternatively, use the NuGet Package Manager for Visual Studio or the NuGet window for JetBrains Rider, then search for _Twilio.AspNet.Core_ and install the package.

## Twilio.AspNet.Mvc

[![NuGet Badge](https://buildstats.info/nuget/Twilio.AspNet.Mvc)](https://www.nuget.org/packages/Twilio.AspNet.Mvc/) 

### Requirements

Requires .NET 4.6.2 or later.

### Installation
From the Package Manager Console or Developer PowerShell, run the following command to install the latest version: 
```PowerShell
Install-Package Twilio.AspNet.Mvc
```
Alternatively, use the NuGet Package Manager for Visual Studio or the NuGet window for JetBrains Rider, then search for _Twilio.AspNet.Mvc_ and install the package.

## Code Samples for either Library

### Incoming SMS
```csharp
using Twilio.AspNet.Common;
using Twilio.AspNet.Core; // or .Mvc for .NET Framework
using Twilio.TwiML;

public class SmsController : TwilioController
{
    // GET: Sms
    public TwiMLResult Index(SmsRequest request)
    {
        var response = new MessagingResponse();
        response.Message($"Ahoy {request.From}!");
        return TwiML(response);
    }
}
```
This controller will handle the SMS webhook. The details of the incoming SMS will be bound to the `SmsRequest request` parameter.
By inheriting from the `TwilioController`, you get access to the `TwiML` method which you can use to respond with TwiML.

### Incoming Voice Call

```csharp
using Twilio.AspNet.Common;
using Twilio.AspNet.Core; // or .Mvc for .NET Framework
using Twilio.TwiML;

public class VoiceController : TwilioController
{
    // GET: Voice
    public TwiMLResult Index(VoiceRequest request)
    {
        var response = new VoiceResponse();
        response.Say($"Ahoy! Are you from {request.FromCity}?");
        return TwiML(response);
    }
}
```
This controller will handle the Voice webhook. The details of the incoming voice call will be bound to the `VoiceRequest request` parameter.
By inheriting from the `TwilioController`, you get access to the `TwiML` method which you can use to respond with TwiML.

### Using `TwiML` extension methods instead of inheriting from `TwilioController`

If you can't inherit from the `TwilioController` class, you can use the `TwiML` extension methods.
```csharp
using Microsoft.AspNetCore.Mvc; // or System.Web.Mvc for .NET Framework
using Twilio.AspNet.Common;
using Twilio.AspNet.Core; // or .Mvc for .NET Framework
using Twilio.TwiML;

public class SmsController : Controller
{
    // GET: Sms
    public TwiMLResult Index(SmsRequest request)
    {
        var response = new MessagingResponse();
        response.Message($"Ahoy {request.From}!");
        return this.TwiML(response);
    }
}
```
This sample is the same as the previous SMS webhook sample, but instead of inheriting from `TwilioController`, the `SmsController` inherits from the ASP.NET MVC provided `Controller`, and uses `this.TwiML` to use the `TwiML` extension method.

### Minimal API
Twilio.AspNet.Core also has support for Minimal APIs.

This sample shows you how you can hande an SMS webhook using HTTP GET and POST.

```csharp
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core.MinimalApi;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/sms", ([FromQuery] string from) =>
{
    var response = new MessagingResponse();
    response.Message($"Ahoy {from}!");
    return Results.Extensions.TwiML(response);
});

app.MapPost("/sms", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var from = form["from"];
    response.Message($"Ahoy {from}!");
    return Results.Extensions.TwiML(response);
});

app.Run();
```

In traditional MVC controllers, the `SmsRequest`, `VoiceRequest`, and other typed request object would be bound, but Minimal APIs does not support the same model binding.    
   
Instead, you can bind individual parameters for HTTP GET requests using the `FromQuery` attribute. When you don't specify the [FromQuery](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis#parameter-binding) attribute, multiple sources will be considered to bind from in addition to the query string parameters. For HTTP POST requests you can grab the form and then retrieve individual parameters by string index.   
To respond with TwiML, use the `Results.Extensions.TwiML` method.

### Model Bind webhook requests to typed .NET objects
Twilio.AspNet comes with multiple classes to help you bind the data from webhooks to strongly typed .NET objects.
Here's the list of classes:
- `SmsRequest`: Holds data for incoming SMS webhook requests
- `SmsStatusCallbackRequest`: Holds data for tracking the delivery status of an outbound Twilio SMS or MMS
- `StatusCallbackRequest`: Holds data for tracking the status of an outbound Twilio Voice Call
- `VoiceRequest`: Holds data for incoming Voice Calls

> **Note**
> Only MVC Controllers and Razor Pages support model binding to typed .NET objects. In Minimal APIs and other scenarios, you'll have to write code to extract the parameters yourself.

The following sample shows how to accept inbound SMS, respond, and track the status of the SMS response.

```csharp
using Twilio.AspNet.Common;
using Twilio.AspNet.Core; // or .Mvc for .NET Framework
using Twilio.TwiML;

public class SmsController : TwilioController
{
    private readonly ILogger<SmsController> logger;

    public SmsController(ILogger<SmsController> logger)
    {
        this.logger = logger;
    }

    public TwiMLResult Index(SmsRequest request)
    {
        var messagingResponse = new MessagingResponse();
        messagingResponse.Message(
            body: $"Ahoy {request.From}!",
            action: new Uri("/Sms/StatusCallback"),
            method: Twilio.Http.HttpMethod.Post
        );
        return TwiML(messagingResponse);
    }

    public void StatusCallback(SmsStatusCallbackRequest request)
    {
        logger.LogInformation("SMS Status: {Status}", request.MessageStatus);
    }
}
```

As shown in the sample above, you can add an `SmsRequest` as a parameter, and MVC will bind the object for you.
The code then responds with an SMS with the `status` and `method` parameter. When the status of the SMS changes, Twilio will send an HTTP POST request to `StatusCallback` action. You can add an `SmsStatusCallbackRequest` as a parameter, and MVC will bind the object for you.

### Add the Twilio client to the ASP.NET Core dependency injection container

In ASP.NET Core, you can add the Twilio REST API client to ASP.NET Core's service using the `.AddTwilioClient` method, like this:

```csharp
using Twilio.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTwilioClient()
```

Now you can request `ITwilioRestClient` and `TwilioRestClient` via dependency injection.

You can configure the Twilio client using the following configuration:

```json
{
  "Twilio": {
    "AuthToken": "[YOUR_AUTH_TOKEN]",
    "Client": {
      "AccountSid": "[YOUR_ACCOUNT_SID]",
      "AuthToken": "[YOUR_AUTH_TOKEN]",
      "ApiKeySid": "[YOUR_API_KEY_SID]",
      "ApiKeySecret": "[YOUR_API_KEY_SECRET]",
      "CredentialType": "[Unspecified|AuthToken|ApiKey]",
      "Region": null,
      "Edge": null,
      "LogLevel": null
    }
  }
}
```

A couple of notes:
- `Twilio:Client:AuthToken` falls back on `Twilio:AuthToken`. You only need to configure one of them.
- `Twilio:Client:CredentialType` has the following valid values: `Unspecified`, `AuthToken`, or `ApiKey`
- `Twilio:Client:CredentialType` is optional and defaults to `Unspecified`. If `Unspecified`, whether you configured an API key or an Auth Token will be detected.

If you do not wish to configure the Twilio client using .NET configuration, you can do so manually:

```csharp
using Twilio.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddTwilioClient((serviceProvider, options) =>
  {
    options.AccountSid = "[YOUR_ACCOUNT_SID]";
    options.AuthToken = "[YOUR_AUTH_TOKEN]";
    options.ApiKeySid = "[YOUR_API_KEY_SID]";
    options.ApiKeySecret = "[YOUR_API_KEY_SECRET]";
    options.Edge = null;
    options.Region = null;
    options.LogLevel = null;
    options.CredentialType = CredentialType.Unspecified;
  });
```

> **Warning**
> Do not hard-code your **Auth Token** or **API key secret** into code and do not check them into source control.
> We recommend using the [Secrets Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development.
> Alternatively, you can use environment variables, a vault service, or other more secure techniques.

#### Use your own HTTP client

By default when you call `.AddTwilioClient`, an HTTP client factory is configured that is used to provide an `HttpClient` to the Twilio REST client. If you'd like to provide your own HTTP client, you can do so by providing a callback like this:

```csharp
```csharp
using Twilio.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTwilioClient(provider => new HttpClient())
```

### Validate Twilio HTTP requests

Webhooks require your endpoint to be publicly available, but this also introduces the risk that bad actors could find your webhook URL and try to abuse it.

Luckily, you can verify that an HTTP request originated from Twilio. 
The `Twilio.AspNet` library provides an attribute that will validate the request for you in MVC.
The implementation differs between the `Twilio.AspNet.Core` and `Twilio.AspNet.Mvc` library.

#### Validate requests in ASP.NET Core MVC

Add the `.AddTwilioRequestValidation` method at startup:

```csharp
using Twilio.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTwilioRequestValidation();
```

Then configure the request validation:

```json
{
  "Twilio": {
    "AuthToken": "[YOUR_AUTH_TOKEN]",
    "RequestValidation": {
      "AuthToken": "[YOUR_AUTH_TOKEN]",
      "AllowLocal": true,
      "BaseUrlOverride": "https://??????.ngrok.io"
    }
  }
}
```

A couple of notes about the configuration:
- `Twilio:RequestValidation:AuthToken` falls back on `Twilio:AuthToken`. You only need to configure one of them.
- `AllowLocal` will skip validation when the HTTP request originated from localhost.
- Use `BaseUrlOverride` in case your app is behind a reverse proxy or a tunnel like ngrok. The path of the current request will be appended to the `BaseUrlOverride` for request validation.

You can also manually configure the request validation:

```csharp
using Twilio.AspNet.Core;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddTwilioRequestValidation((serviceProvider, options) =>
  {
    options.AuthToken = "[YOUR_AUTH_TOKEN]";
    options.AllowLocal = true;
    options.BaseUrlOverride = "https://??????.ngrok.io";
  });
```

> **Warning**
> Do not hard-code your **Auth Token** into code and do not check them into source control.
> We recommend using the [Secrets Manager](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets) for local development.
> Alternatively, you can use environment variables, a vault service, or other more secure techniques.

Now that request validation has been configured, use the `[ValidateRequest]` attribute.
You can apply the attribute globally, to MVC areas, controllers, and actions.
Here's an example where the attribute is applied to the `Index` action:

```csharp
using Twilio.AspNet.Common;
using Twilio.AspNet.Core;
using Twilio.TwiML;

public class SmsController : TwilioController
{
    [ValidateRequest]
    public TwiMLResult Index(SmsRequest request)
    {
        var response = new MessagingResponse();
        response.Message("Ahoy!");
        return TwiML(response);
    }
}
```

#### Validate requests in ASP.NET MVC on .NET Framework

In your _Web.config_ you can configure request validation like shown below:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <configSections>
      <sectionGroup name="twilio" type="Twilio.AspNet.Mvc.TwilioSectionGroup,Twilio.AspNet.Mvc">
        <section name="requestValidation" type="Twilio.AspNet.Mvc.RequestValidationConfigurationSection,Twilio.AspNet.Mvc"/>
      </sectionGroup>
    </configSections>
    <twilio>
       <requestValidation 
         authToken="your auth token here"
         baseUrlOverride="https://??????.ngrok.io"
         allowLocal="true"
       />
    </twilio>
</configuration>
```

You can also configure request validation using app settings:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <appSettings>
      <add key="twilio:requestValidation:authToken" value="[YOUR_AUTH_TOKEN]"/>
      <add key="twilio:requestValidation:baseUrlOverride" value="https://??????.ngrok.io"/>
      <add key="twilio:requestValidation:allowLocal" value="true"/>
    </appSettings>
</configuration>
```

If you configure request validation using both ways, app setting will overwrite the `twilio/requestValidation` configuration element.

A couple of notes about the configuration:
- `allowLocal` will skip validation when the HTTP request originated from localhost.
- Use `baseUrlOverride` in case you are in front of a reverse proxy or a tunnel like ngrok. The path of the current request will be appended to the `baseUrlOverride` for request validation.

> **Warning**
> Do not hard-code your **Auth Token** into code and do not check them into source control.
> Use the `UserSecretsConfigBuilder` for local development or [one of the other configuration builders](https://docs.microsoft.com/en-us/aspnet/config-builder).
> Alternatively, you should encrypt the configuration sections containing secrets like the Auth Token.

Now that request validation has been configured, use the `[ValidateRequest]` attribute.
You can apply the attribute globally, to MVC areas, controllers, and actions. 
Here's an example where the attribute is applied to the `Index` action:

```csharp
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

public class SmsController : TwilioController
{
    [ValidateRequest]
    public TwiMLResult Index(SmsRequest request)
    {
        var response = new MessagingResponse();
        response.Message("Ahoy!");
        return TwiML(response);
    }
}
```

#### Validate requests outside of MVC

The `[ValidateRequest]` attribute only works for MVC. If you need to validate requests outside of MVC, you can use the `RequestValidationHelper` class provided by `Twilio.AspNet`.
Alternatively, the `RequestValidator` class from the [Twilio SDK](https://github.com/twilio/twilio-csharp) can also help you with this.