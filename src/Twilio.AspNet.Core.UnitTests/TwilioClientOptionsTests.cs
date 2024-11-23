using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class TwilioClientOptionsTests
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
            options.LogLevel = client.LogLevel;
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
        var validJson = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        var configuration = new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
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
            .AddInMemoryCollection([
                new KeyValuePair<string, string>("Twilio:AuthToken", ValidTwilioOptions.AuthToken),
                new KeyValuePair<string, string>("Twilio:Client:AccountSid", ValidTwilioOptions.Client.AccountSid)
            ])
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
            Assert.Throws<OptionsValidationException>(() =>
                serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value);
        Assert.Equal("Twilio:Client:{AccountSid|AuthToken} options required for CredentialType.AuthToken.",
            exception.Message);
    }

    [Fact]
    public void AddTwilioClient_With_Missing_ApiKey_Should_Throw()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioClient((_, options) => { options.CredentialType = CredentialType.ApiKey; });

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var exception = Assert.Throws<OptionsValidationException>(
            () => serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value
        );
        Assert.Equal(
            "Twilio:Client:{AccountSid|ApiKeySid|ApiKeySecret} options required for CredentialType.ApiKey.",
            exception.Message);
    }

    [Fact]
    public void AddTwilioClient_AuthToken_Without_Config_Should_Sanitize_Options()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string>("Twilio:Client:AccountSid", "AccountSid"),
                new KeyValuePair<string, string>("Twilio:Client:AuthToken", "AuthToken")
            ]).Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value;

        Assert.Null(options.ApiKeySid);
        Assert.Null(options.ApiKeySecret);
        Assert.Null(options.Region);
        Assert.Null(options.Edge);
        Assert.Null(options.LogLevel);
    }

    [Fact]
    public void AddTwilioClient_ApiKey_Without_Config_Should_Sanitize_Options()
    {
        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new KeyValuePair<string, string>("Twilio:Client:AccountSid", "AccountSid"),
                new KeyValuePair<string, string>("Twilio:Client:ApiKeySid", "ApiKeySid"),
                new KeyValuePair<string, string>("Twilio:Client:ApiKeySecret", "ApiKeySecret")
            ]).Build();
        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TwilioClientOptions>>().Value;

        Assert.Null(options.AuthToken);
        Assert.Null(options.Region);
        Assert.Null(options.Edge);
        Assert.Null(options.LogLevel);
    }

    [Fact]
    public async Task AddTwilioClient_From_Configuration_Should_Reload_On_Change()
    {
        const string optionsFile = "ClientOptions.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(optionsFile, optional: false, reloadOnChange: true)
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        TwilioOptions updatedOptions = new()
        {
            Client = new TwilioClientOptions
            {
                AccountSid = "Updated MyAccountSid!",
                AuthToken = "My Twilio:Client: UpdatedAuthToken",
                ApiKeySid = "My Updated API Key SID",
                ApiKeySecret = "My Updated API Key Secret",
                CredentialType = CredentialType.AuthToken,
                Edge = "MY Updated EDGE",
                Region = "MY Updated REGION"
            }
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = updatedOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        // wait for the option change to be detected
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TwilioClientOptions>>();
        var options = await monitor.WaitForOptionChange();

        var expectedJson = JsonSerializer.Serialize(updatedOptions.Client);
        var actualJson = JsonSerializer.Serialize(options);
        Assert.Equal(expectedJson, actualJson);
    }

    [Fact]
    public async Task AddTwilioClient_From_Configuration_With_Fallback_Should_Reload_On_Change()
    {
        TwilioOptions options = new()
        {
            AuthToken = "My Twilio:AuthToken",
            Client = new TwilioClientOptions
            {
                AccountSid = "MyAccountSid!"
            }
        };

        const string optionsFile = "ClientOptions2.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = options });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        var serviceCollection = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(optionsFile, optional: false, reloadOnChange: true)
            .Build();

        serviceCollection.AddSingleton<IConfiguration>(configuration);
        serviceCollection.AddTwilioClient();

        var serviceProvider = serviceCollection.BuildServiceProvider();

        options = new()
        {
            AuthToken = "My Twilio:Updated AuthToken",
            Client = new TwilioClientOptions
            {
                AccountSid = "MyAccountSid!"
            }
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = options });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        // wait for the option change to be detected
        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<TwilioClientOptions>>();
        var optionsFromDi = await monitor.WaitForOptionChange();

        Assert.Equal(options.AuthToken, optionsFromDi.AuthToken);
    }

    private IConfiguration BuildEmptyConfiguration() => new ConfigurationBuilder().Build();
}