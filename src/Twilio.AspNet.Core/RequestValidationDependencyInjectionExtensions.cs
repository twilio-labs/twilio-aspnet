using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core
{
    public static class RequestValidationDependencyInjectionExtensions
    {
        public static IServiceCollection AddTwilioRequestValidation(this IServiceCollection services)
            => AddTwilioRequestValidation(services, null);


        public static IServiceCollection AddTwilioRequestValidation(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioRequestValidationOptions> configureRequestValidationOptions
        )
        {
            var optionsBuilder = services.AddOptions<TwilioRequestValidationOptions>();
            if (configureRequestValidationOptions != null)
            {
                optionsBuilder.Configure<IServiceProvider>((options, serviceProvider) =>
                    configureRequestValidationOptions(serviceProvider, options));
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

                optionsBuilder.Validate(
                    options => string.IsNullOrEmpty(options.AuthToken) == false,
                    "Twilio:AuthToken or Twilio:RequestValidation:AuthToken option is required."
                );
            }

            return services;
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