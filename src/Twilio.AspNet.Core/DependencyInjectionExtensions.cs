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
                    if (options.Client == null)
                        options.Client = new TwilioClientOptions();
                    if (options.RequestValidation == null)
                        options.RequestValidation = new TwilioRequestValidationOptions();

                    configureOptions(resolver, options);
                    SanitizeOptions(options);
                });

            return services;
        }

        private static void ConfigureDefaultOptions(IServiceProvider serviceProvider, TwilioOptions options)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            configuration.GetSection("Twilio").Bind(options);
        }

        private static void SanitizeOptions(TwilioOptions options)
        {
            var clientOptions = options.Client;
            var requestValidationOptions = options.RequestValidation;

            // properties can be empty strings, but should be set to null if so
            if (options.AuthToken == "") options.AuthToken = null;

            if (requestValidationOptions.AuthToken == "") requestValidationOptions.AuthToken = null;

            if (clientOptions.AccountSid == "") clientOptions.AccountSid = null;
            if (clientOptions.AuthToken == "") clientOptions.AuthToken = null;
            if (clientOptions.ApiKeySid == "") clientOptions.ApiKeySid = null;
            if (clientOptions.ApiKeySecret == "") clientOptions.ApiKeySecret = null;
            if (clientOptions.Region == "") clientOptions.Region = null;
            if (clientOptions.Edge == "") clientOptions.Edge = null;
            if (clientOptions.LogLevel == "") clientOptions.LogLevel = null;

            // if Twilio:Client:AuthToken is not set, fallback on Twilio:AuthToken
            if (clientOptions.AuthToken == null) clientOptions.AuthToken = options.AuthToken;
            // if Twilio:RequestValidation:AuthToken is not set, fallback on Twilio:AuthToken
            if (requestValidationOptions.AuthToken == null) requestValidationOptions.AuthToken = options.AuthToken;

            var isApiKeyConfigured = clientOptions.AccountSid != null &&
                                     clientOptions.ApiKeySid != null &&
                                     clientOptions.ApiKeySecret != null;
            var isAuthTokenConfigured = clientOptions.AccountSid != null &&
                                        clientOptions.AuthToken != null;

            if (clientOptions.CredentialType == CredentialType.Unspecified)
            {
                if (isApiKeyConfigured) clientOptions.CredentialType = CredentialType.ApiKey;
                else if (isAuthTokenConfigured) clientOptions.CredentialType = CredentialType.AuthToken;
            }
            else if (clientOptions.CredentialType == CredentialType.ApiKey && !isApiKeyConfigured)
            {
                throw new Exception(
                    "Twilio:Client:{AccountSid|ApiKeySid|ApiKeySecret} configuration required for CredentialType.ApiKey");
            }
            else if (clientOptions.CredentialType == CredentialType.AuthToken && !isAuthTokenConfigured)
            {
                throw new Exception(
                    "Twilio:Client:{AccountSid|AuthToken} configuration required for CredentialType.AuthToken");
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

            var options = provider.GetService<IOptions<TwilioOptions>>()?.Value?.Client;
            if (options == null)
            {
                throw new Exception("TwilioOptions not found, use AddTwilio or AddTwilioOptions");
            }

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
                    throw new Exception("Twilio:Client not configured");
            }

            if (options.LogLevel != null)
            {
                client.LogLevel = options.LogLevel;
            }

            return client;
        }
    }
}