using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Twilio.Clients;

namespace Twilio.AspNet.Core
{
    public static class DependencyInjectionExtensions
    {
        public static void AddTwilio(
            this IServiceCollection services
        )
        {
            AddTwilio(services, (serviceProvider, options) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                configuration.GetSection("Twilio").Bind(options);
            });
        }

        public static void AddTwilio(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioOptions> configureOptions
        )
        {
            services.AddOptions<TwilioOptions>()
                .Configure<IServiceProvider>((options, resolver) => configureOptions(resolver, options))
                .PostConfigure(options =>
                {
                    // properties can be empty strings, but should be set to null if so
                    if (string.IsNullOrEmpty(options.AccountSid)) options.AccountSid = null;
                    if (string.IsNullOrEmpty(options.AuthToken)) options.AuthToken = null;
                    if (string.IsNullOrEmpty(options.ApiKeySid)) options.ApiKeySid = null;
                    if (string.IsNullOrEmpty(options.ApiKeySecret)) options.ApiKeySecret = null;
                    if (string.IsNullOrEmpty(options.Region)) options.Region = null;
                    if (string.IsNullOrEmpty(options.Edge)) options.Edge = null;
                    if (string.IsNullOrEmpty(options.LogLevel)) options.LogLevel = null;
                    
                    // validate
                    var isApiKeyConfigured = options.AccountSid != null && 
                                             options.ApiKeySid != null &&
                                             options.ApiKeySecret != null;

                    var isAuthTokenConfigured = options.AccountSid != null &&
                                                options.AuthToken != null;

                    if (options.CredentialType == CredentialType.Unspecified)
                    {
                        if (isApiKeyConfigured) options.CredentialType = CredentialType.ApiKey;
                        else if (isAuthTokenConfigured) options.CredentialType = CredentialType.AuthToken;
                        else
                        {
                            throw new Exception("Configure your Twilio API key or Auth Token");
                        }
                    }
                    else if (options.CredentialType == CredentialType.ApiKey && !isApiKeyConfigured)
                    {
                        throw new Exception("Configure Twilio:AccountSid, Twilio:ApiKeySid, and Twilio:ApiKeySecret");
                    }
                    else if (options.CredentialType == CredentialType.AuthToken && !isAuthTokenConfigured)
                    {
                        throw new Exception("Configure Twilio:AccountSid and Twilio:AuthToken");
                    }
                });

            services.AddTransient<ITwilioRestClient>(serviceProvider =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<TwilioOptions>>().Value;
                TwilioRestClient client;
                if (options.CredentialType == CredentialType.ApiKey)
                {
                    client = new TwilioRestClient(
                        username: options.ApiKeySid,
                        password: options.ApiKeySecret,
                        accountSid: options.AccountSid,
                        region: options.Region,
                        httpClient: null,
                        edge: options.Edge
                    );
                }
                else if (options.CredentialType == CredentialType.AuthToken)
                {
                    client = new TwilioRestClient(
                        username: options.AccountSid,
                        password: options.AuthToken,
                        accountSid: options.AccountSid,
                        region: options.Region,
                        httpClient: null,
                        edge: options.Edge
                    );
                }
                else
                {
                    throw new Exception("This code should never be reached.");
                }

                if (options.LogLevel != null)
                {
                    client.LogLevel = options.LogLevel;
                }

                return client;
            });
        }
    }
}