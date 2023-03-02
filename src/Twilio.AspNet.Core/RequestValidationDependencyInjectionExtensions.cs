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
            optionsBuilder.Configure<IConfiguration>((opts, config) =>
            {
                var section = config.GetSection("Twilio");
                if (section.Exists() == false)
                {
                    throw new Exception("Twilio options not configured.");
                }

                ChangeEmptyStringToNull(section);
                section.Bind(opts);
                section = config.GetSection("Twilio:RequestValidation");
                if (section.Exists())
                {
                    ChangeEmptyStringToNull(section);
                    section.Bind(opts);
                }
            });
            
            optionsBuilder.Services.AddSingleton<
                IOptionsChangeTokenSource<TwilioRequestValidationOptions>,
                ConfigurationChangeTokenSource<TwilioRequestValidationOptions>
            >();

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
            
            Validate(optionsBuilder);
            return services;
        }

        private static void Validate(OptionsBuilder<TwilioRequestValidationOptions> optionsBuilder)
        {
            optionsBuilder.Validate(
                options => string.IsNullOrEmpty(options.AuthToken) == false,
                "Twilio:AuthToken or Twilio:RequestValidation:AuthToken option is required."
            );
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