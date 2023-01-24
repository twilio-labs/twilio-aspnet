using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Http;

namespace Twilio.AspNet.Core
{
    public static class TwilioClientDependencyInjectionExtensions
    {
        internal const string TwilioHttpClientName = "Twilio";

        public static IServiceCollection AddTwilioClient(this IServiceCollection services)
            => AddTwilioClient(services, null, null);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioClientOptions> configureTwilioClientOptions
        )
            => AddTwilioClient(services, configureTwilioClientOptions, null);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Func<IServiceProvider, System.Net.Http.HttpClient> provideHttpClient
        )
            => AddTwilioClient(services, null, provideHttpClient);

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioClientOptions> configureTwilioClientOptions,
            Func<IServiceProvider, System.Net.Http.HttpClient> provideHttpClient
        )
        {
            var optionsBuilder = services.AddOptions<TwilioClientOptions>();

            if (configureTwilioClientOptions != null)
            {
                optionsBuilder.Configure<IServiceProvider>((options, serviceProvider) =>
                    configureTwilioClientOptions(serviceProvider, options));
            }
            else
            {
                optionsBuilder.Configure<IConfiguration>((opts, config) =>
                {
                    var section = config.GetSection("Twilio");
                    if (section.Exists() == false)
                    {
                        throw new Exception("Twilio options not configured.");
                    }
                    
                    ChangeEmptyStringToNull(section);
                    section.Bind(opts);
                    section = config.GetSection("Twilio:Client");
                    if (section.Exists() == false)
                    {
                        throw new Exception("Twilio:Client options not configured.");
                    }
                    
                    ChangeEmptyStringToNull(section);
                    section.Bind(opts);
                });
                optionsBuilder.Services.AddSingleton<
                    IOptionsChangeTokenSource<TwilioClientOptions>,
                    ConfigurationChangeTokenSource<TwilioClientOptions>
                >();
            }

            optionsBuilder.PostConfigure(SanitizeTwilioClientOptions);
            
            optionsBuilder.Validate(
                options => options.CredentialType != CredentialType.Unspecified,
                "Twilio:Client:CredentialType could not be determined. Configure as ApiKey or AuthToken."
            );
            optionsBuilder.Validate(options =>
                {
                    var isApiKeyConfigured = options.AccountSid != null &&
                                             options.ApiKeySid != null &&
                                             options.ApiKeySecret != null;
                    return options.CredentialType != CredentialType.ApiKey || isApiKeyConfigured;
                }, "Twilio:Client:{AccountSid|ApiKeySid|ApiKeySecret} options required for CredentialType.ApiKey."
            );
            optionsBuilder.Validate(options =>
                {
                    var isAuthTokenConfigured = options.AccountSid != null &&
                                                options.AuthToken != null;
                    if (options.CredentialType == CredentialType.AuthToken && !isAuthTokenConfigured)
                    {
                        return false;
                    }

                    return true;
                }, "Twilio:Client:{AccountSid|AuthToken} options required for CredentialType.AuthToken."
            );

            if (provideHttpClient == null)
            {
                provideHttpClient = ProvideDefaultHttpClient;

                services.AddHttpClient(TwilioHttpClientName)
                    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                    {
                        // same options as the Twilio C# SDK
                        AllowAutoRedirect = false
                    });
            }

            services.AddScoped<ITwilioRestClient>(provider => CreateTwilioClient(provider, provideHttpClient));
            services.AddScoped<TwilioRestClient>(provider => CreateTwilioClient(provider, provideHttpClient));

            return services;
        }

        private static void SanitizeTwilioClientOptions(TwilioClientOptions options)
        {
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
        }

        private static System.Net.Http.HttpClient ProvideDefaultHttpClient(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(TwilioHttpClientName);

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

            var options = provider.GetRequiredService<IOptionsSnapshot<TwilioClientOptions>>().Value;

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
                    throw new Exception("This code should be unreachable");
            }

            if (options.LogLevel != null)
            {
                client.LogLevel = options.LogLevel;
            }

            return client;
        }


        private static void ChangeEmptyStringToNull(IConfigurationSection configSection)
        {
            if (configSection == null) return;
            if (configSection.Value == "") configSection.Value = null;
            foreach (var childConfigSection in configSection.GetChildren())
            {
                ChangeEmptyStringToNull(childConfigSection);
            }
        }
    }
}