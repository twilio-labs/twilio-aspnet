using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
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

public class TwilioClientTests
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
    public void AddTwilioClient_With_Callback_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildEmptyConfiguration());
        serviceCollection.AddTwilioClient((_, options) =>
        {
            var client = ValidTwilioOptions.Client;
            options.AccountSid = client.AccountSid;
            options.AuthToken = client.AuthToken;
            options.ApiKeySid = client.ApiKeySid;
            options.ApiKeySecret = client.ApiKeySecret;
            options.Edge = client.Edge;
            options.Region = client.Region;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioClientOptions = serviceProvider.GetService<IOptions<TwilioClientOptions>>()?.Value;

        var expectedJson = JsonSerializer.Serialize(ValidTwilioOptions.Client);
        var actualJson = JsonSerializer.Serialize(twilioClientOptions);

        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void AddTwilioClient_With_ValidConfiguration_Should_Match_Configuration()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioClientOptions = serviceProvider.GetService<IOptions<TwilioClientOptions>>()?.Value;

        var expectedJson = JsonSerializer.Serialize(ValidTwilioOptions.Client);
        var actualJson = JsonSerializer.Serialize(twilioClientOptions);

        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public void AddTwilioClient_Should_Fallback_To_AuthToken()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string>("Twilio:AuthToken", ValidTwilioOptions.AuthToken),
                new KeyValuePair<string, string>("Twilio:Client:AccountSid", ValidTwilioOptions.Client.AccountSid)
            })
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioClientOptions = serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value;

        Assert.Equal(ValidTwilioOptions.AuthToken, twilioClientOptions.AuthToken);
    }

    [Fact]
    public void AddTwilioClient_With_AuthToken_Should_Pick_CredentialTypeAuthToken()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, options) =>
        {
            options.AuthToken = ValidTwilioOptions.Client.AuthToken;
            options.AccountSid = ValidTwilioOptions.Client.AccountSid;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioClientOptions = serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value;

        Assert.Equal(CredentialType.AuthToken, twilioClientOptions.CredentialType);
    }

    [Fact]
    public void AddTwilioClient_With_ApiKey_Should_Pick_CredentialTypeApiKey()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, options) =>
        {
            options.ApiKeySid = ValidTwilioOptions.Client.ApiKeySid;
            options.ApiKeySecret = ValidTwilioOptions.Client.ApiKeySecret;
            options.AccountSid = ValidTwilioOptions.Client.AccountSid;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var twilioClientOptions = serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value;

        Assert.Equal(CredentialType.ApiKey, twilioClientOptions.CredentialType);
    }

    [Fact]
    public void AddTwilioClient_With_Missing_AuthToken_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, options) => { options.CredentialType = CredentialType.AuthToken; });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception =
            Assert.Throws<Exception>(() => serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value);
        Assert.Equal("Twilio:Client:{AccountSid|AuthToken} configuration required for CredentialType.AuthToken.",
            exception.Message);
    }

    [Fact]
    public void AddTwilioClient_With_Missing_ApiKey_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, options) => { options.CredentialType = CredentialType.ApiKey; });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception =
            Assert.Throws<Exception>(() => serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value);
        Assert.Equal(
            "Twilio:Client:{AccountSid|ApiKeySid|ApiKeySecret} configuration required for CredentialType.ApiKey.",
            exception.Message);
    }

    [Fact]
    public void AddTwilioClient_With_ValidOptions_Should_AddTwilioClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());
        serviceCollection.AddTwilioClient();

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
    public void AddTwilioClient_With_ApiKeyOptions_Should_Match_Properties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildApiKeyConfiguration());
        serviceCollection.AddTwilioClient();

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
    public void AddTwilioClient_With_AuthTokenOptions_Should_Match_Properties()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildAuthTokenConfiguration());
        serviceCollection.AddTwilioClient();

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
    
    public void AddTwilioClient_Without_HttpClientProvider_Should_Named_HttpClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());

        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var twilioRestClient = scope.ServiceProvider.GetService<TwilioRestClient>();
        
        var actualHttpClient = (System.Net.Http.HttpClient) typeof(SystemNetHttpClient)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(twilioRestClient.HttpClient);

        Assert.NotNull(actualHttpClient);
        // need better assertions, but not sure how
    }

    [Fact]
    public void AddTwilioClient_With_HttpClientProvider_Should_Use_HttpClient()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildValidConfiguration());

        using var httpClient = new System.Net.Http.HttpClient();
        serviceCollection.AddTwilioClient(_ => httpClient);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var twilioRestClient = scope.ServiceProvider.GetService<TwilioRestClient>();
        var httpClientFromTwilioClient = (System.Net.Http.HttpClient) typeof(SystemNetHttpClient)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(twilioRestClient.HttpClient);

        Assert.Equal(httpClient, httpClientFromTwilioClient);
    }

    [Fact]
    public void AddTwilioClient_Without_Configuration_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<Exception>(() => scope.ServiceProvider.GetService<ITwilioRestClient>());
        Assert.Equal("IConfiguration not found.", exception.Message);
    }
    
    [Fact]
    public void AddTwilioClient_With_Empty_Configuration_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton(BuildEmptyConfiguration());
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<Exception>(() => scope.ServiceProvider.GetService<ITwilioRestClient>());
        Assert.Equal("Twilio:Client not configured.", exception.Message);
    }

    [Fact]
    public void AddTwilioClient_Without_ClientOptions_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, _) => { });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<Exception>(() => scope.ServiceProvider.GetService<ITwilioRestClient>());
        Assert.Equal("Twilio:Client not configured.", exception.Message);
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