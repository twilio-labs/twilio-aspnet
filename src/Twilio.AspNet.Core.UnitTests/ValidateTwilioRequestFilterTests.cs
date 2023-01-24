using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class ValidateTwilioRequestFilterTests
{
    private static readonly TwilioOptions ValidTwilioOptions = new()
    {
        AuthToken = "My Twilio:AuthToken",
        RequestValidation = new TwilioRequestValidationOptions
        {
            AuthToken = "My Twilio:RequestValidation:AuthToken",
            AllowLocal = false,
            BaseUrlOverride = "MY URL OVERRIDE"
        }
    };
    
    [Fact]
    public void AddTwilioRequestValidation_Should_Configure_ValidateTwilioRequestFilter()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioRequestValidation((_, options) =>
        {
            options.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
            options.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
            options.BaseUrlOverride = ValidTwilioOptions.RequestValidation.BaseUrlOverride;
        });
        
        serviceCollection.AddTransient<ValidateTwilioRequestFilter>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var filter = serviceProvider.GetRequiredService<ValidateTwilioRequestFilter>();

        Assert.Equal(ValidTwilioOptions.RequestValidation.AllowLocal, filter.AllowLocal);
        Assert.Equal(ValidTwilioOptions.RequestValidation.AuthToken, filter.AuthToken);
        Assert.Equal(ValidTwilioOptions.RequestValidation.BaseUrlOverride, filter.BaseUrlOverride);
    }
    
    [Fact]
    public async Task ValidateRequestFilter_Should_Use_Reloaded_Configuration()
    {
        const string optionsFile = "ValidateRequestFilterAutoReload.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(optionsFile, optional: false, reloadOnChange: true)
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();
        serviceCollection.AddTransient<ValidateTwilioRequestFilter>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var filter = serviceProvider.GetRequiredService<ValidateTwilioRequestFilter>();
        
        Assert.Equal(ValidTwilioOptions.RequestValidation.AuthToken, filter.AuthToken);
        Assert.Equal(ValidTwilioOptions.RequestValidation.BaseUrlOverride, filter.BaseUrlOverride);
        Assert.Equal(ValidTwilioOptions.RequestValidation.AllowLocal, filter.AllowLocal);

        
        TwilioOptions updatedOptions = new()
        {
            RequestValidation = new TwilioRequestValidationOptions
            {
                AuthToken = "My Twilio:RequestValidation:Updated Auth Token",
                AllowLocal = true,
                BaseUrlOverride = "Different URL"
            }
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = updatedOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);
        
        // wait for the option change to be detected
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TwilioRequestValidationOptions>>();
        await monitor.WaitForOptionChange();

        // IOptionsSnapshot is calculated per scope
        using var scope = serviceProvider.CreateScope();
        filter = scope.ServiceProvider.GetRequiredService<ValidateTwilioRequestFilter>();
        
        Assert.Equal(updatedOptions.RequestValidation.AuthToken, filter.AuthToken);
        Assert.Equal(updatedOptions.RequestValidation.BaseUrlOverride, filter.BaseUrlOverride);
        Assert.Equal(updatedOptions.RequestValidation.AllowLocal,filter.AllowLocal);
    }
    
    [Fact]
    public void Creating_ValidateTwilioRequestFilter_Without_AddTwilioClient_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<ValidateTwilioRequestFilter>();
        var serviceProvider = serviceCollection.BuildServiceProvider();

        var exception = Assert.Throws<Exception>(() => serviceProvider.GetRequiredService<ValidateTwilioRequestFilter>());
        Assert.Equal("TwilioRequestValidationOptions is not configured.", exception.Message);
    }

    [Fact]
    public async Task ValidateRequestFilter_Validates_Request_Successfully()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioRequestValidation((_, options) =>
        {
            options.AllowLocal = false;
            options.AuthToken = ContextMocks.fakeAuthToken;
            options.BaseUrlOverride = null;
        });
        serviceCollection.AddTransient<ValidateTwilioRequestFilter>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var filter = serviceProvider.GetRequiredService<ValidateTwilioRequestFilter>();
        
        var form = new FormCollection(new Dictionary<string, StringValues>
        {
            {"key1", "value1"},
            {"key2", "value2"}
        });
        var fakeContext = new ContextMocks(true, form).HttpContext.Object;

        var result = await filter.InvokeAsync(
            new DefaultEndpointFilterInvocationContext(fakeContext), 
            _ => ValueTask.FromResult<object>(Results.Ok())
        );

        Assert.IsType<Ok>(result);
    }

    [Fact]
    public async Task ValidateTwilioRequestFilter_Validates_Request_Forbid()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioRequestValidation((_, options) =>
        {
            options.AllowLocal = false;
            options.AuthToken = "bad";
            options.BaseUrlOverride = null;
        });
        serviceCollection.AddTransient<ValidateTwilioRequestFilter>();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var filter = serviceProvider.GetRequiredService<ValidateTwilioRequestFilter>();
        
        var form = new FormCollection(new Dictionary<string, StringValues>
        {
            {"key1", "value1"},
            {"key2", "value2"}
        });
        var fakeContext = new ContextMocks(true, form).HttpContext.Object;

        var result = await filter.InvokeAsync(
            new DefaultEndpointFilterInvocationContext(fakeContext), 
            _ => ValueTask.FromResult<object>(Results.Ok())
        );

        var statusCodeResult = (StatusCodeHttpResult)result!;
        Assert.NotNull(statusCodeResult);
        Assert.Equal((int)HttpStatusCode.Forbidden, statusCodeResult.StatusCode);
    }
}