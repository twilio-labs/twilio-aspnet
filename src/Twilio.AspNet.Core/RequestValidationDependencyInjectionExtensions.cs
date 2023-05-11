using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core
{
    public static class RequestValidationDependencyInjectionExtensions
    {
        public static IServiceCollection AddTwilioRequestValidation(this IServiceCollection services)
        {
            var optionsBuilder = services.AddOptions<TwilioRequestValidationOptions>();
            optionsBuilder.Configure<IConfiguration>((options, config) =>
            {
                var twilioSection = config.GetSection("Twilio");
                if (twilioSection.Exists() == false)
                {
                    throw new Exception("Twilio options not configured.");
                }

                var requestValidationSection = config.GetSection("Twilio:RequestValidation");
                requestValidationSection.Bind(options);

                var authTokenFallback = twilioSection["AuthToken"];
                if (string.IsNullOrEmpty(options.AuthToken) && !string.IsNullOrEmpty(authTokenFallback))
                    options.AuthToken = authTokenFallback;
            });
            optionsBuilder.Services.AddSingleton<
                IOptionsChangeTokenSource<TwilioRequestValidationOptions>,
                ConfigurationChangeTokenSource<TwilioRequestValidationOptions>
            >();
            Sanitize(optionsBuilder); 
            Validate(optionsBuilder); 
            return services;
        }

        public static IServiceCollection AddTwilioRequestValidation(
            this IServiceCollection services,
            IConfiguration namedConfigurationSection
        )
        {
            var optionsBuilder = services.AddOptions<TwilioRequestValidationOptions>();
            optionsBuilder.Bind(namedConfigurationSection);
            Validate(optionsBuilder);
            Sanitize(optionsBuilder);
            return services;
        }


        public static IServiceCollection AddTwilioRequestValidation(
            this IServiceCollection services,
            Action<TwilioRequestValidationOptions> configureOptions
        )
            => AddTwilioRequestValidation(services, (_, options) => configureOptions(options));

        public static IServiceCollection AddTwilioRequestValidation(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioRequestValidationOptions> configureOptions
        )
        {
            var optionsBuilder = services.AddOptions<TwilioRequestValidationOptions>();
            optionsBuilder.Configure<IServiceProvider>((options, provider) => configureOptions(provider, options));
            Sanitize(optionsBuilder);
            Validate(optionsBuilder);
            return services;
        }

        public static IServiceCollection AddTwilioRequestValidation(
            this IServiceCollection services,
            TwilioRequestValidationOptions options
        )
        {
            var optionsBuilder = services.AddOptions<TwilioRequestValidationOptions>();
            optionsBuilder.Configure<IServiceProvider>((optionsToConfigure, _) =>
            {
                optionsToConfigure.AuthToken = options.AuthToken;
                optionsToConfigure.AllowLocal = options.AllowLocal;
                optionsToConfigure.BaseUrlOverride = options.BaseUrlOverride;
            });
            Sanitize(optionsBuilder);
            Validate(optionsBuilder);
            return services;
        }

        private static void Sanitize(OptionsBuilder<TwilioRequestValidationOptions> optionsBuilder)
        {
            optionsBuilder.PostConfigure(options =>
            {
                if (options.AuthToken == "") options.AuthToken = null;
                if (options.BaseUrlOverride == "") options.BaseUrlOverride = null;
                if (options.BaseUrlOverride != null) options.BaseUrlOverride = options.BaseUrlOverride.TrimEnd('/');
            });
        }

        private static void Validate(OptionsBuilder<TwilioRequestValidationOptions> optionsBuilder)
        {
            optionsBuilder.Validate(
                options => string.IsNullOrEmpty(options.AuthToken) == false,
                "Twilio:AuthToken or Twilio:RequestValidation:AuthToken option is required."
            );
        }
    }
}