using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Http;

namespace Twilio.AspNet.Core
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddTwilioClient(this IServiceCollection services)
            => AddTwilioClient(services, ConfigureDefaultTwilioClientOptions, null);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioClientOptions> configureTwilioClientOptions
        )
            => AddTwilioClient(services, configureTwilioClientOptions, null);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Func<IServiceProvider, System.Net.Http.HttpClient> provideHttpClient
        )
            => AddTwilioClient(services, ConfigureDefaultTwilioClientOptions, provideHttpClient);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioClientOptions> configureTwilioClientOptions,
            Func<IServiceProvider, System.Net.Http.HttpClient> provideHttpClient
        )
        {
            services.AddOptions<TwilioClientOptions>()
                .Configure<IServiceProvider>((options, serviceProvider) =>
                {
                    configureTwilioClientOptions(serviceProvider, options);
                    SanitizeTwilioClientOptions(options);
                });

            services.AddTransient<ITwilioRestClient>(provider => CreateTwilioClient(provider, provideHttpClient));
            services.AddTransient<TwilioRestClient>(provider => CreateTwilioClient(provider, provideHttpClient));

            return services;
        }

        private static void ConfigureDefaultTwilioClientOptions(
            IServiceProvider serviceProvider,
            TwilioClientOptions options
        )
        {
            var configuration = serviceProvider.GetService<IConfiguration>();
            if (configuration == null)
            {
                throw new Exception("IConfiguration not found.");
            }

            var section = configuration.GetSection("Twilio:Client");
            if (section.Exists() == false)
            {
                throw new Exception("Twilio:Client not configured.");
            }

            section.Bind(options);

            // if Twilio:Client:AuthToken is not set, fallback on Twilio:AuthToken
            if (string.IsNullOrEmpty(options.AuthToken)) options.AuthToken = configuration["Twilio:AuthToken"];
        }

        private static void SanitizeTwilioClientOptions(TwilioClientOptions options)
        {
            // properties can be empty strings, but should be set to null if so
            if (options.AccountSid == "") options.AccountSid = null;
            if (options.AuthToken == "") options.AuthToken = null;
            if (options.ApiKeySid == "") options.ApiKeySid = null;
            if (options.ApiKeySecret == "") options.ApiKeySecret = null;
            if (options.Region == "") options.Region = null;
            if (options.Edge == "") options.Edge = null;
            if (options.LogLevel == "") options.LogLevel = null;

            var isApiKeyConfigured = options.AccountSid != null &&
                                     options.ApiKeySid != null &&
                                     options.ApiKeySecret != null;
            var isAuthTokenConfigured = options.AccountSid != null &&
                                        options.AuthToken != null;

            if (options.CredentialType == CredentialType.Unspecified)
            {
                if (isApiKeyConfigured) options.CredentialType = CredentialType.ApiKey;
                else if (isAuthTokenConfigured) options.CredentialType = CredentialType.AuthToken;
            }
            else if (options.CredentialType == CredentialType.ApiKey && !isApiKeyConfigured)
            {
                throw new Exception(
                    "Twilio:Client:{AccountSid|ApiKeySid|ApiKeySecret} configuration required for CredentialType.ApiKey.");
            }
            else if (options.CredentialType == CredentialType.AuthToken && !isAuthTokenConfigured)
            {
                throw new Exception(
                    "Twilio:Client:{AccountSid|AuthToken} configuration required for CredentialType.AuthToken.");
            }
        }

        private static TwilioRestClient CreateTwilioClient(
            IServiceProvider provider,
            Func<IServiceProvider, System.Net.Http.HttpClient> provideHttpClient
        )
        {
            Twilio.Http.HttpClient twilioHttpClient = null;
            if (provideHttpClient != null)
            {
                var httpClient = provideHttpClient(provider);
                twilioHttpClient = new SystemNetHttpClient(httpClient);
            }

            var options = provider.GetRequiredService<IOptions<TwilioClientOptions>>().Value;

            TwilioRestClient client;
            switch (options.CredentialType)
            {
                case CredentialType.ApiKey:
                    client = new TwilioRestClient(
                        username: options.ApiKeySid,
                        password: options.ApiKeySecret,
                        accountSid: options.AccountSid,
                        region: options.Region,
                        httpClient: twilioHttpClient,
                        edge: options.Edge
                    );
                    break;
                case CredentialType.AuthToken:
                    client = new TwilioRestClient(
                        username: options.AccountSid,
                        password: options.AuthToken,
                        accountSid: options.AccountSid,
                        region: options.Region,
                        httpClient: twilioHttpClient,
                        edge: options.Edge
                    );
                    break;
                default:
                    throw new Exception("Twilio:Client not configured.");
            }

            if (options.LogLevel != null)
            {
                client.LogLevel = options.LogLevel;
            }

            return client;
        }
    }
}