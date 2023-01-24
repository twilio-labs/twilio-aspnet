using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
        Client = new TwilioClientOptions
        {
            AccountSid = "MyAccountSid!",
            AuthToken = "My Twilio:Client:AuthToken",
            ApiKeySid = "My API Key SID",
            ApiKeySecret = "My API Key Secret",
            CredentialType = CredentialType.ApiKey,
            Edge = "MY EDGE",
            Region = "MY REGION",
            LogLevel = "debug"
        }
    };

    private static readonly TwilioOptions AuthTokenTwilioOptions = new()
    {
        Client = new TwilioClientOptions
        {
            AccountSid = "MyAccountSid!",
            AuthToken = "My Twilio:Client:AuthToken",
            CredentialType = CredentialType.AuthToken,
            Edge = "MY EDGE",
            Region = "MY REGION",
            LogLevel = "debug"
        }
    };

    private static readonly TwilioOptions ApiKeyTwilioOptions = new()
    {
        Client = new TwilioClientOptions
        {
            AccountSid = "MyAccountSid!",
            ApiKeySid = "My API Key SID",
            ApiKeySecret = "My API Key Secret",
            CredentialType = CredentialType.ApiKey,
            Edge = "MY EDGE",
            Region = "MY REGION",
            LogLevel = "debug"
        }
    };

    [Fact]
    public void AddTwilioClient_With_ValidOptions_Should_AddTwilioClient()
    {
        var host = BuildValidHost();
        using var scope = host.Services.CreateScope();
        var twilioRestClients = new[]
        {
            scope.ServiceProvider.GetService<TwilioRestClient>(),
            (TwilioRestClient)scope.ServiceProvider.GetService<ITwilioRestClient>()
        };
        foreach (var client in twilioRestClients)
        {
            Assert.NotNull(client);
        }
    }

    [Fact]
    public void AddTwilioClient_With_ApiKeyOptions_Should_Match_Properties()
    {
        var host = BuildApiKeyHost();
        using var scope = host.Services.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<TwilioRestClient>();
        Assert.Equal(ApiKeyTwilioOptions.Client.Region, client.Region);
        Assert.Equal(ApiKeyTwilioOptions.Client.Edge, client.Edge);
        Assert.Equal(ApiKeyTwilioOptions.Client.AccountSid, client.AccountSid);
        Assert.Equal(ApiKeyTwilioOptions.Client.LogLevel, client.LogLevel);
        Assert.Equal(ApiKeyTwilioOptions.Client.ApiKeySid,
            typeof(TwilioRestClient).GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(client));
        Assert.Equal(ApiKeyTwilioOptions.Client.ApiKeySecret,
            typeof(TwilioRestClient).GetField("_password", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(client));
    }

    [Fact]
    public void AddTwilioClient_With_AuthTokenOptions_Should_Match_Properties()
    {
        var host = BuildAuthTokenHost();
        using var scope = host.Services.CreateScope();

        var client = scope.ServiceProvider.GetRequiredService<TwilioRestClient>();
        Assert.Equal(AuthTokenTwilioOptions.Client.Region, client.Region);
        Assert.Equal(AuthTokenTwilioOptions.Client.Edge, client.Edge);
        Assert.Equal(AuthTokenTwilioOptions.Client.AccountSid, client.AccountSid);
        Assert.Equal(AuthTokenTwilioOptions.Client.LogLevel, client.LogLevel);
        Assert.Equal(AuthTokenTwilioOptions.Client.AccountSid,
            typeof(TwilioRestClient).GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(client));
        Assert.Equal(AuthTokenTwilioOptions.Client.AuthToken,
            typeof(TwilioRestClient).GetField("_password", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(client));
    }

    [Fact]
    public async Task AddTwilioClient_Should_Use_Reloaded_Configuration()
    {
        const string optionsFile = "AddTwilioClientAutoReload.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        var host = new HostBuilder()
            .ConfigureAppConfiguration(builder =>
                builder.AddJsonFile(optionsFile, optional: false, reloadOnChange: true))
            .ConfigureServices(services => services.AddTwilioClient())
            .Build();
        
        using (var scope = host.Services.CreateScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<TwilioRestClient>();

            Assert.Equal(ValidTwilioOptions.Client.Region, client.Region);
            Assert.Equal(ValidTwilioOptions.Client.Edge, client.Edge);
            Assert.Equal(ValidTwilioOptions.Client.AccountSid, client.AccountSid);
            Assert.Equal(ValidTwilioOptions.Client.LogLevel, client.LogLevel);
            Assert.Equal(ValidTwilioOptions.Client.ApiKeySid,
                typeof(TwilioRestClient).GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(client));
            Assert.Equal(ValidTwilioOptions.Client.ApiKeySecret,
                typeof(TwilioRestClient).GetField("_password", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(client));
        }

        TwilioOptions updatedOptions = new()
        {
            Client = new TwilioClientOptions
            {
                AccountSid = "MyAccountSid updated!",
                AuthToken = "My Twilio:Client:AuthToken updated",
                ApiKeySid = "My API Key SID updated",
                ApiKeySecret = "My API Key Secret updated",
                CredentialType = CredentialType.AuthToken,
                Edge = "MY EDGE updated",
                Region = "MY REGION updated",
                LogLevel = null
            }
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = updatedOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        // wait for the option change to be detected
        var monitor = host.Services.GetRequiredService<IOptionsMonitor<TwilioClientOptions>>();
        await monitor.WaitForOptionChange();

        // IOptionsSnapshot is calculated per scope
        using (var scope = host.Services.CreateScope())
        {
            var client = scope.ServiceProvider.GetRequiredService<TwilioRestClient>();

            Assert.Equal(updatedOptions.Client.Region, client.Region);
            Assert.Equal(updatedOptions.Client.Edge, client.Edge);
            Assert.Equal(updatedOptions.Client.AccountSid, client.AccountSid);
            Assert.Equal(updatedOptions.Client.LogLevel, client.LogLevel);
            Assert.Equal(updatedOptions.Client.AccountSid,
                typeof(TwilioRestClient).GetField("_username", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(client));
            Assert.Equal(updatedOptions.Client.AuthToken,
                typeof(TwilioRestClient).GetField("_password", BindingFlags.NonPublic | BindingFlags.Instance)!
                    .GetValue(client));
        }
    }

    [Fact]
    public void AddTwilioClient_Without_HttpClientProvider_Should_Named_HttpClient()
    {
        var host = BuildValidHost();
        using var scope = host.Services.CreateScope();

        var twilioRestClient = scope.ServiceProvider.GetService<TwilioRestClient>();

        var actualHttpClient = (System.Net.Http.HttpClient)typeof(SystemNetHttpClient)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(twilioRestClient.HttpClient);

        Assert.NotNull(actualHttpClient);
    }

    [Fact]
    public void AddTwilioClient_With_HttpClientProvider_Should_Use_HttpClient()
    {
        var validJson = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);

        using var httpClient = new System.Net.Http.HttpClient();
        // ReSharper disable once AccessToDisposedClosure
        serviceCollection.AddTwilioClient(_ => httpClient);

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var twilioRestClient = scope.ServiceProvider.GetService<TwilioRestClient>();
        var httpClientFromTwilioClient = (System.Net.Http.HttpClient)typeof(SystemNetHttpClient)
            .GetField("_httpClient", BindingFlags.NonPublic | BindingFlags.Instance)!
            .GetValue(twilioRestClient.HttpClient);

        Assert.Equal(httpClient, httpClientFromTwilioClient);
    }

    [Fact]
    public void AddTwilioClient_With_Empty_Configuration_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<Exception>(() => scope.ServiceProvider.GetService<ITwilioRestClient>());
        Assert.Equal("Twilio options not configured.", exception.Message);
    }

    [Fact]
    public void AddTwilioClient_Without_ClientOptions_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, _) => { });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<OptionsValidationException>(
            () => scope.ServiceProvider.GetService<ITwilioRestClient>()
        );
        Assert.Equal(
            "Twilio:Client:CredentialType could not be determined. Configure as ApiKey or AuthToken.",
            exception.Message
        );
    }

    [Fact]
    public void AddTwilioClient_Without_Sufficient_Options_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, options) =>
        {
            options.AccountSid = "";
            options.ApiKeySid = "";
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        var exception = Assert.Throws<OptionsValidationException>(
            () => scope.ServiceProvider.GetService<ITwilioRestClient>()
        );
        Assert.Equal(
            "Twilio:Client:CredentialType could not be determined. Configure as ApiKey or AuthToken.",
            exception.Message
        );
    }

    private static IHost BuildValidHost() => BuildHost(ValidTwilioOptions);
    private static IHost BuildAuthTokenHost() => BuildHost(AuthTokenTwilioOptions);
    private static IHost BuildApiKeyHost() => BuildHost(ApiKeyTwilioOptions);

    private static IHost BuildHost(TwilioOptions options)
    {
        var validJson = JsonSerializer.Serialize(new { Twilio = options });
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        return new HostBuilder()
            .ConfigureAppConfiguration(builder =>
            {
                builder.AddJsonStream(jsonStream);
            })
            .ConfigureServices(services => services.AddTwilioClient())
            .Build();
    }
}