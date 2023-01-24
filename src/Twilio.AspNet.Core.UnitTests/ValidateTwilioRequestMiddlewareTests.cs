using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Twilio.AspNet.Core.UnitTests;

public class ValidateTwilioRequestMiddlewareTests
{
    private static readonly TwilioOptions ValidTwilioOptions = new()
    {
        AuthToken = "My Twilio:AuthToken",
        RequestValidation = new TwilioRequestValidationOptions
        {
            AuthToken = "My Twilio:RequestValidation:AuthToken",
            AllowLocal = false,
            BaseUrlOverride = "https://example.localhost"
        }
    };

    [Fact]
    public async Task UseTwilioRequestValidation_Should_Validate_Request_Successfully()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddTwilioRequestValidation((_, options) =>
                    {
                        options.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
                        options.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
                        options.BaseUrlOverride = ValidTwilioOptions.RequestValidation.BaseUrlOverride;
                    });
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseTwilioRequestValidation();
                    app.UseRouting();
                    app.UseEndpoints(builder => { builder.MapPost("/sms", () => Results.Ok()); });
                })
            )
            .StartAsync();

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
            c.Request.Headers.Add(
                "X-Twilio-Signature",
                CalculateSignature(
                    $"{ValidTwilioOptions.RequestValidation.BaseUrlOverride}{c.Request.Path}",
                    ValidTwilioOptions.RequestValidation.AuthToken,
                    c.Request.Form
                )
            );
        });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task UseTwilioRequestValidation_Should_Validate_Request_Forbid()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddTwilioRequestValidation((_, options) =>
                    {
                        options.AllowLocal = ValidTwilioOptions.RequestValidation.AllowLocal;
                        options.AuthToken = ValidTwilioOptions.RequestValidation.AuthToken;
                        options.BaseUrlOverride = ValidTwilioOptions.RequestValidation.BaseUrlOverride;
                    });
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseTwilioRequestValidation();
                    app.UseRouting();
                    app.UseEndpoints(builder => { builder.MapPost("/sms", () => Results.Ok()); });
                })
            )
            .StartAsync();

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
            c.Request.Headers.Add("X-Twilio-Signature", "sldflsjf");
        });

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }

    [Fact]
    public async Task UseTwilioRequestValidation_Should_Validate_Request_Successfully_With_AutoReload()
    {
        const string optionsFile = "ValidateRequestMiddlewareAutoReload2.json";
        if (File.Exists(optionsFile)) File.Delete(optionsFile);
        var jsonText = JsonSerializer.Serialize(new { Twilio = ValidTwilioOptions });
        await File.WriteAllTextAsync(optionsFile, jsonText);

        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureAppConfiguration(builder =>
                    builder.AddJsonFile(optionsFile, optional: false, reloadOnChange: true))
                .ConfigureServices(services =>
                {
                    services.AddTwilioRequestValidation();
                    services.AddRouting();
                })
                .Configure(app =>
                {
                    app.UseTwilioRequestValidation();
                    app.UseRouting();
                    app.UseEndpoints(builder => { builder.MapPost("/sms", () => Results.Ok()); });
                })
            )
            .StartAsync();

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
            c.Request.Headers.Add(
                "X-Twilio-Signature",
                CalculateSignature(
                    $"{ValidTwilioOptions.RequestValidation.BaseUrlOverride}{c.Request.Path}",
                    ValidTwilioOptions.RequestValidation.AuthToken,
                    c.Request.Form
                )
            );
        });

        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        
        TwilioOptions updatedOptions = new()
        {
            RequestValidation = new TwilioRequestValidationOptions
            {
                AuthToken = "My Twilio:RequestValidation:Updated Auth Token",
                AllowLocal = false,
                BaseUrlOverride = "https://example.local"
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
            c.Request.Headers.Add(
                "X-Twilio-Signature",
                CalculateSignature(
                    $"{updatedOptions.RequestValidation.BaseUrlOverride}{c.Request.Path}",
                    updatedOptions.RequestValidation.AuthToken,
                    c.Request.Form
                )
            );
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
            c.Request.Headers.Add("X-Twilio-Signature", "sdfsjf");
        });
        
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }
    
    private string CalculateSignature(string url, string authToken, IFormCollection form)
    {
        var value = new StringBuilder(url);
        if (form != null)
        {
            var sortedKeys = form.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();
            foreach (var key in sortedKeys)
            {
                value.Append(key);
                value.Append(form[key]);
            }
        }

        var sha1 = new HMACSHA1(Encoding.UTF8.GetBytes(authToken));
        var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(value.ToString()));

        return Convert.ToBase64String(hash);
    }
}