using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class ValidateTwilioRequestTests
{
    private static readonly TwilioOptions ValidTwilioOptions = new()
    {
        AuthToken = "My Twilio:AuthToken",
        RequestValidation = new TwilioRequestValidationOptions
        {
            AuthToken = "My Twilio:RequestValidation:AuthToken",
            AllowLocal = false,
            BaseUrlOverride = "https://example.localhost/"
        }
    };

    [Theory]
    [InlineData(typeof(ValidateRequestAttribute))]
    [InlineData(typeof(ValidateTwilioRequestFilter))]
    [InlineData(typeof(ValidateTwilioRequestMiddleware))]
    public async Task Validate_Request_Successfully(Type type)
    {
        var validJson = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        var host = await BuildHost(type, builder => builder.AddJsonStream(jsonStream));

        var server = host.GetTestServer();
        server.BaseAddress = new Uri("https://example.com/");

        var context = await server.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/sms";
            c.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "From", "+1234567890" },
                { "Body", "Ahoy!" }
            });
            c.Request.Headers["X-Twilio-Signature"] = ValidationHelper.CalculateSignature(
                    $"{ValidTwilioOptions.RequestValidation.BaseUrlOverride.TrimEnd('/')}{c.Request.Path}",
                    ValidTwilioOptions.RequestValidation.AuthToken,
                    c.Request.Form
                );
        });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Theory]
    [InlineData(typeof(ValidateRequestAttribute))]
    [InlineData(typeof(ValidateTwilioRequestFilter))]
    [InlineData(typeof(ValidateTwilioRequestMiddleware))]
    public async Task Validate_Request_Forbid(Type type)
    {
        var validJson = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        using var jsonStream = new MemoryStream(Encoding.UTF8.GetBytes(validJson));
        var host = await BuildHost(type, builder => builder.AddJsonStream(jsonStream));

        var server = host.GetTestServer();
        server.BaseAddress = new Uri("https://example.com/");

        var context = await server.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/sms";
            c.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "From", "+1234567890" },
                { "Body", "Ahoy!" }
            });
            c.Request.Headers["X-Twilio-Signature"] = "sldflsjf";
        });

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Theory]
    [InlineData(typeof(ValidateRequestAttribute))]
    [InlineData(typeof(ValidateTwilioRequestFilter))]
    [InlineData(typeof(ValidateTwilioRequestMiddleware))]
    public async Task Validate_Request_With_ReloadOnChange(Type type)
    {
        const string optionsFile = "ValidateRequestMiddlewareAutoReload2.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        using var host = await BuildHost(
            type,
            builder => builder.AddJsonFile(optionsFile, optional: false, reloadOnChange: true)
        );

        var server = host.GetTestServer();
        server.BaseAddress = new Uri("https://example.com/");

        var context = await server.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/sms";
            c.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "From", "+1234567890" },
                { "Body", "Ahoy!" }
            });
            c.Request.Headers["X-Twilio-Signature"] = ValidationHelper.CalculateSignature(
                    $"{ValidTwilioOptions.RequestValidation.BaseUrlOverride.TrimEnd('/')}{c.Request.Path}",
                    ValidTwilioOptions.RequestValidation.AuthToken,
                    c.Request.Form
                )
;
        });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);

        TwilioOptions updatedOptions = new()
        {
            RequestValidation = new TwilioRequestValidationOptions
            {
                AuthToken = "My Twilio:RequestValidation:Updated Auth Token",
                AllowLocal = false,
                BaseUrlOverride = "https://example.local/"
            }
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = updatedOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        // wait for the option change to be detected
        var monitor = host.Services.GetRequiredService<IOptionsMonitor<TwilioRequestValidationOptions>>();
        await monitor.WaitForOptionChange();

        context = await server.SendAsync(c =>
        {
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/sms";
            c.Request.Form = new FormCollection(new Dictionary<string, StringValues>
            {
                { "From", "+1234567890" },
                { "Body", "Ahoy!" }
            });
            c.Request.Headers["X-Twilio-Signature"] = ValidationHelper.CalculateSignature(
                    $"{updatedOptions.RequestValidation.BaseUrlOverride.TrimEnd('/')}{c.Request.Path}",
                    updatedOptions.RequestValidation.AuthToken,
                    c.Request.Form
                )
;
        });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);

        updatedOptions = new()
        {
            RequestValidation = new TwilioRequestValidationOptions
            {
                AuthToken = "asdfds",
                AllowLocal = true,
                BaseUrlOverride = "sdfds"
            }
        };

        jsonText = JsonSerializer.Serialize(new { Twilio = updatedOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        // wait for the option change to be detected
        await monitor.WaitForOptionChange();

        context = await server.SendAsync(c =>
        {
            c.Request.Host = new HostString("localhost");
            c.Request.Method = HttpMethods.Post;
            c.Request.Path = "/sms";
            c.Request.Headers["X-Twilio-Signature"] = "sdfsjf";
        });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static Task<IHost> BuildHost(Type type, Action<IConfigurationBuilder> buildConfig) => new HostBuilder()
        .ConfigureWebHost(webBuilder => webBuilder
            .UseTestServer()
            .ConfigureAppConfiguration(buildConfig)
            .ConfigureServices(services =>
            {
                services.AddTwilioRequestValidation();
                if (type == typeof(ValidateRequestAttribute))
                {
                    services.AddControllers();
                }
                else
                {
                    services.AddRouting();
                }
            })
            .Configure(app =>
            {
                app.UseTwilioRequestValidation();
                app.UseRouting();
                app.UseEndpoints(builder =>
                {
                    if (type == typeof(ValidateRequestAttribute))
                    {
                        builder.MapControllers();
                    }
                    else if (type == typeof(ValidateTwilioRequestFilter))
                    {
                        builder.MapPost("/sms", () => Results.Ok())
                            .ValidateTwilioRequest();
                    }
                    else if (type == typeof(ValidateTwilioRequestMiddleware))
                    {
                        builder.MapPost("/sms", () => Results.Ok());
                    }
                });
            })
        )
        .StartAsync();
}

public class SmsController : Controller
{
    [HttpPost("/sms")]
    public void Sms() => Ok();
}