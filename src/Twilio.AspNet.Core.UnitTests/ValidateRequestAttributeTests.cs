using System.IO;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class ValidateRequestAttributeTests
{
    private static readonly TwilioOptions ValidTwilioOptions = new()
    {
        AuthToken = "My Twilio:AuthToken",
        RequestValidation = new TwilioRequestValidationOptions()
        {
            AuthToken = "My Twilio:RequestValidation:AuthToken",
            AllowLocal = false,
            UrlOverride = "MY URL OVERRIDE"
        }
    };

    [Fact]
    public void AddTwilio_Should_Configure_ValidateRequestAttribute()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioOptions((_, options) =>
        {
            options.RequestValidation.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
            options.RequestValidation.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
            options.RequestValidation.UrlOverride = ValidTwilioOptions.RequestValidation.UrlOverride;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var attributeFactory = new ValidateRequestAttribute();
        var attribute =
            (ValidateRequestAttribute.InternalValidateRequestAttribute) attributeFactory
                .CreateInstance(serviceProvider);

        Assert.Equal(ValidTwilioOptions.RequestValidation.AllowLocal, attribute.AllowLocal);
        Assert.Equal(ValidTwilioOptions.RequestValidation.AuthToken, attribute.AuthToken);
        Assert.Equal(ValidTwilioOptions.RequestValidation.UrlOverride, attribute.UrlOverride);
    }

    [Fact]
    public void ValidateRequestAttribute_Properties_Should_Override_TwilioOptions()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTwilioOptions((_, options) =>
        {
            options.RequestValidation.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
            options.RequestValidation.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
            options.RequestValidation.UrlOverride = ValidTwilioOptions.RequestValidation.UrlOverride;
        });

        var serviceProvider = serviceCollection.BuildServiceProvider();

        var allowLocal = true;
        var authToken = "Different Auth Token!";
        var urlOverride = "Different URL Override";

        var attributeFactory = new ValidateRequestAttribute
        {
            AllowLocal = allowLocal,
            AuthToken = authToken,
            UrlOverride = urlOverride
        };

        var attribute =
            (ValidateRequestAttribute.InternalValidateRequestAttribute) attributeFactory
                .CreateInstance(serviceProvider);

        Assert.Equal(allowLocal, attribute.AllowLocal);
        Assert.Equal(authToken, attribute.AuthToken);
        Assert.Equal(urlOverride, attribute.UrlOverride);
    }

    private IConfiguration BuildValidConfiguration()
    {
        var validJson = JsonSerializer.Serialize(new {Twilio = ValidTwilioOptions});
        var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        return new ConfigurationBuilder()
            .AddJsonStream(jsonStream)
            .Build();
    }
}