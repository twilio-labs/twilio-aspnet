using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class ValidateRequestAttributeTests
{
    private static readonly TwilioOptions ValidTwilioOptions = new()
    {
        AuthToken = "My Twilio:AuthToken",
        RequestValidation = new TwilioRequestValidationOptions()
        {
            AuthToken = "My Twilio:RequestValidation:AuthToken",
            AllowLocal = false,
            BaseUrlOverride = "MY URL OVERRIDE"
        }
    };

    [Fact]
    public void AddTwilioRequestValidation_With_Callback_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioRequestValidation((_, options) =>
        {
            var requestValidation = ValidTwilioOptions.RequestValidation;
            options.AuthToken = requestValidation.AuthToken;
            options.AllowLocal = requestValidation.AllowLocal;
            options.BaseUrlOverride = requestValidation.BaseUrlOverride;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioRequestValidationOptions =
            serviceProvider.GetService<IOptions<TwilioRequestValidationOptions>>()?.Value;

        var expectedJson = JsonSerializer.Serialize(ValidTwilioOptions.RequestValidation);
        var actualJson = JsonSerializer.Serialize(twilioRequestValidationOptions);

        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void AddTwilioRequestValidation_Should_Fallback_To_AuthToken()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Twilio:AuthToken", ValidTwilioOptions.AuthToken)
            })
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value;

        Assert.Equal(ValidTwilioOptions.AuthToken, options.AuthToken);
    }

    [Fact]
    public void AddTwilio_Should_Configure_ValidateRequestAttribute()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioRequestValidation((_, options) =>
        {
            options.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
            options.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
            options.BaseUrlOverride = ValidTwilioOptions.RequestValidation.BaseUrlOverride;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var attributeFactory = new ValidateRequestAttribute();
        var attribute =
            (ValidateRequestAttribute.InternalValidateRequestAttribute) attributeFactory
                .CreateInstance(serviceProvider);

        Assert.Equal(ValidTwilioOptions.RequestValidation.AllowLocal, attribute.AllowLocal);
        Assert.Equal(ValidTwilioOptions.RequestValidation.AuthToken, attribute.AuthToken);
        Assert.Equal(ValidTwilioOptions.RequestValidation.BaseUrlOverride, attribute.BaseUrlOverride);
    }

    [Fact]
    public void Creating_ValidateRequestAttribute_Without_AddTwilioClient_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var attributeFactory = new ValidateRequestAttribute();
        var exception = Assert.Throws<Exception>(() =>
            (ValidateRequestAttribute.InternalValidateRequestAttribute) attributeFactory
                .CreateInstance(serviceProvider));
        Assert.Equal("RequestValidationOptions is not configured.", exception.Message);
    }


    [Fact]
    public void ValidateRequestAttribute_Validates_Request_Successfully()
    {
        var attribute = new ValidateRequestAttribute.InternalValidateRequestAttribute(
            authToken: ContextMocks.fakeAuthToken, 
            null, 
            false
        );

        var form = new FormCollection(new Dictionary<string, StringValues>()
        {
            {"key1", "value1"},
            {"key2", "value2"}
        });
        var fakeContext = new ContextMocks(true, form).HttpContext.Object;

        var actionExecutingContext = new ActionExecutingContext(
            new ActionContext(fakeContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object()
        );
        
        attribute.OnActionExecuting(actionExecutingContext);
        
        Assert.Null(actionExecutingContext.Result);
    }

    [Fact]
    public void ValidateRequestFilter_Validates_Request_Forbid()
    {
        var attribute = new ValidateRequestAttribute.InternalValidateRequestAttribute(
            authToken: "bad", 
            null, 
            false
        );

        var form = new FormCollection(new Dictionary<string, StringValues>()
        {
            {"key1", "value1"},
            {"key2", "value2"}
        });
        var fakeContext = new ContextMocks(true, form).HttpContext.Object;

        var actionExecutingContext = new ActionExecutingContext(
            new ActionContext(fakeContext, new RouteData(), new ActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            new object()
        );
        
        attribute.OnActionExecuting(actionExecutingContext);

        var statusCodeResult = (StatusCodeResult)actionExecutingContext.Result!;
        Assert.NotNull(statusCodeResult);
        Assert.Equal((int)HttpStatusCode.Forbidden, statusCodeResult.StatusCode);
    }
}