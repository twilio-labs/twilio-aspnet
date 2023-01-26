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
        {
            var optionsBuilder = services.AddOptions<TwilioClientOptions>();

            ConfigureDefaultOptions(optionsBuilder);
            PostConfigure(optionsBuilder);
            Validate(optionsBuilder);
            AddServices(services);

            return services;
        }

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            IConfiguration namedConfigurationSection
        )
        {
            var optionsBuilder = services.AddOptions<TwilioClientOptions>();
            optionsBuilder.Bind(namedConfigurationSection);
            PostConfigure(optionsBuilder);
            Validate(optionsBuilder);
            AddServices(services);

            return services;
        }

        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Action<TwilioClientOptions> configureOptions
        )
            => AddTwilioClient(services, (_, options) => configureOptions(options));


        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioClientOptions> configureOptions
        )
        {
            var optionsBuilder = services.AddOptions<TwilioClientOptions>();

            optionsBuilder.Configure<IServiceProvider>((options, provider) => configureOptions(provider, options));
            PostConfigure(optionsBuilder);
            Validate(optionsBuilder);
            AddServices(services);

            return services;
        }


        public static IServiceCollection AddTwilioClient(
            this IServiceCollection services,
            TwilioClientOptions options
        )
        {
            var optionsBuilder = services.AddOptions<TwilioClientOptions>();

            optionsBuilder.Configure<IServiceProvider>((optionsToConfigure, _) =>
            {
                optionsToConfigure.AccountSid = options.AccountSid;
                optionsToConfigure.AuthToken = options.AuthToken;
                optionsToConfigure.ApiKeySid = options.ApiKeySid;
                optionsToConfigure.ApiKeySecret = options.ApiKeySecret;
                optionsToConfigure.CredentialType = options.CredentialType;
                optionsToConfigure.Edge = options.Edge;
                optionsToConfigure.Region = options.Region;
                optionsToConfigure.LogLevel = options.LogLevel;
            });
            PostConfigure(optionsBuilder);
            Validate(optionsBuilder);
            AddServices(services);

            return services;
        }

        private static void ConfigureDefaultOptions(OptionsBuilder<TwilioClientOptions> optionsBuilder)
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

        private static void PostConfigure(OptionsBuilder<TwilioClientOptions> optionsBuilder)
            => optionsBuilder.PostConfigure(ConfigureCredentialType);

        private static void AddServices(IServiceCollection services)
        {
            services.AddHttpClient(TwilioHttpClientName)
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    // same options as the Twilio C# SDK
                    AllowAutoRedirect = false
                });

            services.AddScoped<ITwilioRestClient>(CreateTwilioClient);
            services.AddScoped<TwilioRestClient>(CreateTwilioClient);
        }

        private static void Validate(OptionsBuilder<TwilioClientOptions> optionsBuilder)
        {
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
        }

        private static void ConfigureCredentialType(TwilioClientOptions options)
        {
            if (options.CredentialType != CredentialType.Unspecified) return;

            var isApiKeyConfigured = options.AccountSid != null &&
                                     options.ApiKeySid != null &&
                                     options.ApiKeySecret != null;
            var isAuthTokenConfigured = options.AccountSid != null &&
                                        options.AuthToken != null;

            if (isApiKeyConfigured) options.CredentialType = CredentialType.ApiKey;
            else if (isAuthTokenConfigured) options.CredentialType = CredentialType.AuthToken;
        }

        private static TwilioRestClient CreateTwilioClient(IServiceProvider provider)
        {
            var httpClient = provider.GetRequiredService<IHttpClientFactory>().CreateClient(TwilioHttpClientName);
            Twilio.Http.HttpClient twilioHttpClient = new SystemNetHttpClient(httpClient);

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
                case CredentialType.Unspecified:
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