# Twilio helper library for ASP.NET

[![Build status](https://ci.appveyor.com/api/projects/status/813hnjynh8ncamwj?svg=true)](https://ci.appveyor.com/project/TwilioAPI/twilio-aspnet) 

**The Twilio helper library for ASP.NET (Twilio.AspNet), helps you integrate the official [Twilio SDK for C# and .NET](https://github.com/twilio/twilio-csharp) into your ASP.NET applications.** The library supports ASP.NET MVC 3-5 on .NET Framework and ASP.NET Core 1+.

You only need this library if you wish to respond to Twilio webhooks for
voice calls and SMS messages. If you only need to use the Twilio REST API's,
then you only need the [Twilio SDK for C# and .NET](https://github.com/twilio/twilio-csharp).

## Twilio.AspNet.Core
[![NuGet Badge](https://buildstats.info/nuget/Twilio.AspNet.Core)](https://www.nuget.org/packages/Twilio.AspNet.Core/) 
### Requirements

Requires .NET Core 1.0 or later with ASP.NET Core 1.0 or later.

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

Requires .NET 4.5.1 or later with ASP.NET MVC 3-5.

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
        response.Message(
            $"Hey there {request.From}! " +
            "How 'bout those Seahawks?"
        );
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
        response.Say($"Welcome. Are you from {request.FromCity}?");
        return TwiML(response);
    }
}
```
This controller will handle the Voice webhook. The details of the incoming voice call will be bound to the `VoiceRequest request` parameter.
By inheriting from the `TwilioController`, you get access to the `TwiML` method which you can use to respond with TwiML.

### Using `TwiML` extension methods instead of inheriting from `TwilioController`

If you can't inherit from the `TwilioController` class, you can use the `TwiML` extension methods.
```csharp
using Twilio.AspNet.Common;
using Twilio.AspNet.Core; // or .Mvc for .NET Framework
using Twilio.TwiML;

public class SmsController : Controller
{
    // GET: Sms
    public TwiMLResult Index(SmsRequest request)
    {
        var response = new MessagingResponse();
        response.Message(
            $"Hey there {request.From}! " +
            "How 'bout those Seahawks?"
        );
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
    response.Message(
        $"Hey there {from}! " +
        "How 'bout those Seahawks?"
    );
    return Results.Extensions.TwiML(response);
});

app.MapPost("/sms", (HttpRequest request) =>
{
    var from = request.Form["From"];
    var response = new MessagingResponse();
    response.Message(
        $"Hey there {from}! " +
        "How 'bout those Seahawks?"
    );
    return Results.Extensions.TwiML(response);
});

app.Run();
```
In traditional MVC controllers, the `SmsRequest`, `VoiceRequest`, and other typed request object would be bound, but Minimal APIs does not support the same model binding. Instead, you can bind individual parameters for HTTP GET requests using the `FromQuery` attribute. For HTTP POST requests you can grab the parameters from the `HttpRequest.Form` collection.   
To respond with TwiML, use the `Results.Extensions.TwiML` method.

### Model Bind webhook requests to typed .NET objects
Twilio.AspNet comes with multiple classes to help you bind the data from webhooks to strongly typed .NET objects.
Here's the list of classes:
- `SmsRequest`: Holds data for incoming SMS webhook requests
- `SmsStatusCallbackRequest`: Holds data for tracking the delivery status of an outbound Twilio SMS or MMS
- `StatusCallbackRequest`: Holds data for tracking the status of an outbound Twilio Voice Call
- `VoiceRequest`: Holds data for incoming Voice Calls

Note: Only MVC Controllers and Razor Pages supports model binding to typed .NET objects. In Minimal APIs and other scenario's, you'll have to write code to extract the parameters yourself.

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
			body: $"Hey there {request.From}! How 'bout those Seahawks?",
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
