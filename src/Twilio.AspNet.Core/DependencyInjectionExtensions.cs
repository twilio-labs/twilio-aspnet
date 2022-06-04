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
        public static IServiceCollection AddTwilio(this IServiceCollection services)
        {
            AddTwilioOptions(services, ConfigureDefaultOptions);
            AddTwilioClient(services);

            return services;
        }

        public static IServiceCollection AddTwilioOptions(this IServiceCollection services)
            => AddTwilioOptions(services, ConfigureDefaultOptions);

        public static IServiceCollection AddTwilioOptions(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioOptions> configureOptions
        )
        {
            services.AddOptions<TwilioOptions>()
                .Configure<IServiceProvider>((options, resolver) =>
                {
                    configureOptions(resolver, options);
                    ValidateOptions(options);
                });

            return services;
        }

        private static void ConfigureDefaultOptions(IServiceProvider serviceProvider, TwilioOptions options)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            configuration.GetSection("Twilio").Bind(options);
        }

        private static void ValidateOptions(TwilioOptions options)
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
                else throw new Exception("Configure your Twilio API key or Auth Token");
            }
            else if (options.CredentialType == CredentialType.ApiKey && !isApiKeyConfigured)
            {
                throw new Exception("Configure Twilio:AccountSid, Twilio:ApiKeySid, and Twilio:ApiKeySecret");
            }
            else if (options.CredentialType == CredentialType.AuthToken && !isAuthTokenConfigured)
            {
                throw new Exception("Configure Twilio:AccountSid and Twilio:AuthToken");
            }
        }

        public static IServiceCollection AddTwilioClient(this IServiceCollection services)
            => AddTwilioClient(services, provideHttpClient: null);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Func<IServiceProvider, System.Net.Http.HttpClient> provideHttpClient
        )
        {
            services.AddTransient<ITwilioRestClient>(provider => CreateTwilioClient(provider, provideHttpClient));
            services.AddTransient<TwilioRestClient>(provider => CreateTwilioClient(provider, provideHttpClient));

            return services;
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

            var options = provider.GetRequiredService<IOptions<TwilioOptions>>().Value;
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
                    throw new Exception("This code should never be reached.");
            }

            if (options.LogLevel != null)
            {
                client.LogLevel = options.LogLevel;
            }

            return client;
        }
    }
}