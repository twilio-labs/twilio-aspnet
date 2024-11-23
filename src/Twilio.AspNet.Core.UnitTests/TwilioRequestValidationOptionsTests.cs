using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class AddTwilioRequestValidationOptionsTests
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
    public void AddTwilioRequestValidation_From_Configuration_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string>("Twilio:AuthToken", ValidTwilioOptions.AuthToken),
                new KeyValuePair<string, string>(
                    "Twilio:RequestValidation:AuthToken", ValidTwilioOptions.RequestValidation.AuthToken),
                new KeyValuePair<string, string>(
                    "Twilio:RequestValidation:BaseUrlOverride", ValidTwilioOptions.RequestValidation.BaseUrlOverride),
                new KeyValuePair<string, string>(
                    "Twilio:RequestValidation:AllowLocal", ValidTwilioOptions.RequestValidation.AllowLocal.ToString())
            ])
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value;

        var expectedJson = JsonSerializer.Serialize(ValidTwilioOptions.RequestValidation);
        var actualJson = JsonSerializer.Serialize(options);

        Assert.Equal(expectedJson, actualJson);
    }
    
    [Fact]
    public void AddTwilioRequestValidation_From_ConfigurationSection_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string>(
                    "Twilio:AuthToken", ValidTwilioOptions.RequestValidation.AuthToken),
                new KeyValuePair<string, string>(
                    "Twilio:BaseUrlOverride", ValidTwilioOptions.RequestValidation.BaseUrlOverride),
                new KeyValuePair<string, string>(
                    "Twilio:AllowLocal", ValidTwilioOptions.RequestValidation.AllowLocal.ToString())
            ])
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation(configuration.GetSection("Twilio"));

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value;

        var expectedJson = JsonSerializer.Serialize(ValidTwilioOptions.RequestValidation);
        var actualJson = JsonSerializer.Serialize(options);

        Assert.Equal(expectedJson, actualJson);
    }
    
    [Fact]
    public void AddTwilioRequestValidation_From_Options_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioRequestValidation(ValidTwilioOptions.RequestValidation);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value;

        var expectedJson = JsonSerializer.Serialize(ValidTwilioOptions.RequestValidation);
        var actualJson = JsonSerializer.Serialize(options);

        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void AddTwilioRequestValidation_Should_Fallback_To_AuthToken()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string>("Twilio:AuthToken", ValidTwilioOptions.AuthToken)
            ])
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value;

        Assert.Equal(ValidTwilioOptions.AuthToken, options.AuthToken);
    }

    [Fact]
    public void AddTwilioRequestValidation_Without_Config_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception = Assert.Throws<Exception>(
            () => serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value
        );

        Assert.Equal("Twilio options not configured.", exception.Message);
    }

    [Fact]
    public void AddTwilioRequestValidation_Without_AuthToken_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string>("Twilio", null),
                new KeyValuePair<string, string>("Twilio:RequestValidation:AuthToken", null)
            ]).Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception = Assert.Throws<OptionsValidationException>(
            () => serviceProvider.GetRequiredService<IOptions<TwilioRequestValidationOptions>>().Value
        );

        Assert.Equal(
            "Twilio:AuthToken or Twilio:RequestValidation:AuthToken option is required.",
            exception.Message
        );
    }

    [Fact]
    public async Task AddTwilioRequestValidation_From_Configuration_Should_Reload_On_Change()
    {
        const string optionsFile = "ValidationOptions.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(optionsFile, optional: false, reloadOnChange: true)
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();

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
        var options = await monitor.WaitForOptionChange();

        var expectedJson = JsonSerializer.Serialize(updatedOptions.RequestValidation);
        var actualJson = JsonSerializer.Serialize(options);
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public async Task AddTwilioRequestValidation_From_Configuration_With_Fallback_Should_Reload_On_Change()
    {
        TwilioOptions options = new()
        {
            AuthToken = "My Twilio:AuthToken",
            RequestValidation = new TwilioRequestValidationOptions()
        };

        const string optionsFile = "ValidationOptions2.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = options });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(optionsFile, optional: false, reloadOnChange: true)
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioRequestValidation();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        options = new()
        {
            AuthToken = "My Twilio:Updated AuthToken",
            RequestValidation = new TwilioRequestValidationOptions()
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = options });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        // wait for the option change to be detected
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TwilioRequestValidationOptions>>();
        var optionsFromDi = await monitor.WaitForOptionChange();

        Assert.Equal(options.AuthToken, optionsFromDi.AuthToken);
    }
}