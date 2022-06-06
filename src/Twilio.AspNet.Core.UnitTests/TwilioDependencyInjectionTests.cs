using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Http;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class TwilioDependencyInjectionTests
{
    private static readonly TwilioOptions ValidTwilioOptions = new()
    {
        AuthToken = "My Twilio:AuthToken",
        Client = new TwilioClientOptions()
        {
            AccountSid = "MyAccountSid!",
            AuthToken = "My Twilio:Client:AuthToken",
            ApiKeySid = "My API Key SID",
            ApiKeySecret = "My API Key Secret",
            CredentialType = CredentialType.ApiKey,
            Edge = "MY EDGE",
            Region = "MY REGION"
        },
        RequestValidation = new TwilioRequestValidationOptions()
        {
            AuthToken = "My Twilio:RequestValidation:AuthToken",
            AllowLocal = true,
            UrlOverride = "MY URL OVERRIDE"
        }
    };

    private static readonly TwilioOptions AuthTokenTwilioOptions = new()
    {
        Client = new TwilioClientOptions()
        {
            AccountSid = "MyAccountSid!",
            AuthToken = "My Twilio:Client:AuthToken",
            CredentialType = CredentialType.AuthToken,
            Edge = "MY EDGE",
            Region = "MY REGION"
        }
    };

    private static readonly TwilioOptions ApiKeyTwilioOptions = new()
    {
        Client = new TwilioClientOptions()
        {
            AccountSid = "MyAccountSid!",
            ApiKeySid = "My API Key SID",
            ApiKeySecret = "My API Key Secret",
            CredentialType = CredentialType.ApiKey,
            Edge = "MY EDGE",
            Region = "MY REGION"
        }
    };

    [Fact]
    public void AddTwilio_Should_AddTwilioOptions()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildEmptyConfiguration());
        serviceCollection.AddTwilio();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetService<IOptions<TwilioOptions>>()?.Value;

        Assert.NotNull(twilioOptions);
    }

    [Fact]
    public void AddTwilioOptions_Should_AddTwilioOptions()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildEmptyConfiguration());
        serviceCollection.AddTwilioOptions();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetService<IOptions<TwilioOptions>>()?.Value;

        Assert.NotNull(twilioOptions);
    }

    [Fact]
    public void AddTwilioOptions_With_Callback_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildEmptyConfiguration());
        serviceCollection.AddTwilioOptions((_, options) =>
        {
            options.AuthToken = ValidTwilioOptions.AuthToken;

            options.Client.AccountSid = ValidTwilioOptions.Client.AccountSid;
            options.Client.AuthToken = ValidTwilioOptions.Client.AuthToken;
            options.Client.ApiKeySid = ValidTwilioOptions.Client.ApiKeySid;
            options.Client.ApiKeySecret = ValidTwilioOptions.Client.ApiKeySecret;
            options.Client.Edge = ValidTwilioOptions.Client.Edge;
            options.Client.Region = ValidTwilioOptions.Client.Region;

            options.RequestValidation.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
            options.RequestValidation.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
            options.RequestValidation.UrlOverride = ValidTwilioOptions.RequestValidation.UrlOverride;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetService<IOptions<TwilioOptions>>()?.Value;

        var validOptionsJson = JsonSerializer.Serialize(ValidTwilioOptions);
        var optionsJson = JsonSerializer.Serialize(twilioOptions);

        Assert.Equal(validOptionsJson, optionsJson);
    }

    [Fact]
    public void AddTwilioOptions_With_ValidConfiguration_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());
        serviceCollection.AddTwilioOptions();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetService<IOptions<TwilioOptions>>()?.Value;

        var validOptionsJson = JsonSerializer.Serialize(ValidTwilioOptions);
        var optionsJson = JsonSerializer.Serialize(twilioOptions);

        Assert.Equal(validOptionsJson, optionsJson);
    }

    [Fact]
    public void AddTwilioOptions_Should_Fallback_To_AuthToken()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Twilio:AuthToken", ValidTwilioOptions.AuthToken)
            })
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioOptions();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value;

        Assert.Equal(ValidTwilioOptions.AuthToken, twilioOptions.AuthToken);
        Assert.Equal(ValidTwilioOptions.AuthToken, twilioOptions.Client.AuthToken);
        Assert.Equal(ValidTwilioOptions.AuthToken, twilioOptions.RequestValidation.AuthToken);
    }

    [Fact]
    public void AddTwilioOptions_With_AuthToken_Should_Pick_CredentialTypeAuthToken()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioOptions((_, options) =>
        {
            options.Client.AuthToken = ValidTwilioOptions.Client.AuthToken;
            options.Client.AccountSid = ValidTwilioOptions.Client.AccountSid;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value;

        Assert.Equal(CredentialType.AuthToken, twilioOptions.Client.CredentialType);
    }

    [Fact]
    public void AddTwilioOptions_With_ApiKey_Should_Pick_CredentialTypeApiKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioOptions((_, options) =>
        {
            options.Client.ApiKeySid = ValidTwilioOptions.Client.ApiKeySid;
            options.Client.ApiKeySecret = ValidTwilioOptions.Client.ApiKeySecret;
            options.Client.AccountSid = ValidTwilioOptions.Client.AccountSid;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioOptions = serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value;

        Assert.Equal(CredentialType.ApiKey, twilioOptions.Client.CredentialType);
    }

    [Fact]
    public void AddTwilioOptions_With_Missing_AuthToken_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioOptions((_, options) =>
        {
            options.Client.CredentialType = CredentialType.AuthToken;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception =
            Assert.Throws<Exception>(() => serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value);
        Assert.Equal("Twilio:Client:{AccountSid|AuthToken} configuration required for CredentialType.AuthToken", exception.Message);
    }

    [Fact]
    public void AddTwilioOptions_With_Missing_ApiKey_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioOptions((_, options) => { options.Client.CredentialType = CredentialType.ApiKey; });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception = Assert.Throws<Exception>(() => serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value);
        Assert.Equal("Twilio:Client:{AccountSid|ApiKeySid|ApiKeySecret} configuration required for CredentialType.ApiKey", exception.Message);
    }

    [Fact]
    public void AddTwilioClient_WithValidOptions_Should_AddTwilioClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());
        serviceCollection
            .AddTwilioOptions()
            .AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var twilioRestClients = new[]
        {
            scope.ServiceProvider.GetService<TwilioRestClient>(),
            (TwilioRestClient) scope.ServiceProvider.GetService<ITwilioRestClient>()
        };
        foreach (var client in twilioRestClients)
        {
            Assert.NotNull(client);
        }
    }

    [Fact]
    public void AddTwilioClient_WithApiKeyOptions_Should_Match_Properties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildApiKeyConfiguration());
        serviceCollection
            .AddTwilioOptions()
            .AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<TwilioRestClient>();
        Assert.Equal(ValidTwilioOptions.Client.Region, client.Region);
        Assert.Equal(ValidTwilioOptions.Client.Edge, client.Edge);
        Assert.Equal(ValidTwilioOptions.Client.AccountSid, client.AccountSid);
        Assert.Equal(ValidTwilioOptions.Client.LogLevel, client.LogLevel);
        Assert.Equal(ValidTwilioOptions.Client.ApiKeySid,
            typeof(TwilioRestClient).GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(client));
        Assert.Equal(ValidTwilioOptions.Client.ApiKeySecret,
            typeof(TwilioRestClient).GetField("_password", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(client));
    }

    [Fact]
    public void AddTwilioClient_WithAuthTokenOptions_Should_Match_Properties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildAuthTokenConfiguration());
        serviceCollection
            .AddTwilioOptions()
            .AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<TwilioRestClient>();
        Assert.Equal(ValidTwilioOptions.Client.Region, client.Region);
        Assert.Equal(ValidTwilioOptions.Client.Edge, client.Edge);
        Assert.Equal(ValidTwilioOptions.Client.AccountSid, client.AccountSid);
        Assert.Equal(ValidTwilioOptions.Client.LogLevel, client.LogLevel);
        Assert.Equal(ValidTwilioOptions.Client.AccountSid,
            typeof(TwilioRestClient).GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(client));
        Assert.Equal(ValidTwilioOptions.Client.AuthToken,
            typeof(TwilioRestClient).GetField("_password", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(client));
    }

    [Fact]
    public void AddTwilioClient_WithHttpClientProvider_Should_Use_HttpClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());

        using var httpClient = new System.Net.Http.HttpClient();
        serviceCollection
            .AddTwilioOptions()
            .AddTwilioClient(_ => httpClient);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var twilioRestClient = scope.ServiceProvider.GetService<TwilioRestClient>();
        var httpClientFromTwilioClient = (System.Net.Http.HttpClient) typeof(SystemNetHttpClient)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(twilioRestClient.HttpClient);

        Assert.Equal(httpClient, httpClientFromTwilioClient);
    }

    [Fact]
    public void AddTwilioClient_Without_Options_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<Exception>(() => scope.ServiceProvider.GetService<ITwilioRestClient>());
        Assert.Equal("TwilioOptions not found, use AddTwilio or AddTwilioOptions", exception.Message);
    }

    [Fact]
    public void AddTwilioClient_Without_ClientOptions_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection
            .AddTwilioOptions((_, _) => { })
            .AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<Exception>(() => scope.ServiceProvider.GetService<ITwilioRestClient>());
        Assert.Equal("Twilio:Client not configured", exception.Message);
    }

    private IConfiguration BuildEmptyConfiguration() => new ConfigurationBuilder().Build();

    private IConfiguration BuildValidConfiguration()
    {
        var validJson = JsonSerializer.Serialize(new {Twilio = ValidTwilioOptions});
        var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        return new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
    }

    private IConfiguration BuildAuthTokenConfiguration()
    {
        var validJson = JsonSerializer.Serialize(new {Twilio = AuthTokenTwilioOptions});
        var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        return new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
    }

    private IConfiguration BuildApiKeyConfiguration()
    {
        var validJson = JsonSerializer.Serialize(new {Twilio = ApiKeyTwilioOptions});
        var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        return new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
    }
}