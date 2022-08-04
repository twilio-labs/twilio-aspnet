using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Twilio.AspNet.Core
{
    public static class RequestValidationDependencyInjectionExtensions
    {
        public static IServiceCollection AddTwilioRequestValidation(this IServiceCollection services)
            => AddTwilioRequestValidation(services, ConfigureDefaultRequestValidation);


        public static IServiceCollection AddTwilioRequestValidation(
            this IServiceCollection services,
            Action<IServiceProvider, TwilioRequestValidationOptions> configureRequestValidationOptions
        )
        {
            services.AddOptions<TwilioRequestValidationOptions>()
                .Configure<IServiceProvider>((options, serviceProvider) =>
                {
                    configureRequestValidationOptions(serviceProvider, options);
                    SanitizeTwilioRequestValidationOptions(options);
                });

            return services;
        }

        private static void ConfigureDefaultRequestValidation(
            IServiceProvider serviceProvider,
            TwilioRequestValidationOptions options
        )
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            configuration.GetSection("Twilio:RequestValidation").Bind(options);

            // if Twilio:RequestValidation:AuthToken is not set, fallback on Twilio:AuthToken
            if (string.IsNullOrEmpty(options.AuthToken)) options.AuthToken = configuration["Twilio:AuthToken"];
        }

        private static void SanitizeTwilioRequestValidationOptions(TwilioRequestValidationOptions options)
        {
            // properties can be empty strings, but should be set to null if so
            if (options.AuthToken == "") options.AuthToken = null;
            if (options.BaseUrlOverride == "") options.BaseUrlOverride = null;
        }
    }
}