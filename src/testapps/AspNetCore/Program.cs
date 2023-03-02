using Microsoft.AspNetCore.HttpOverrides;
using System.Xml.Linq;
using Twilio.AspNet.Core;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTwilioClient()
    .AddTwilioRequestValidation();

builder.Services.Configure<ForwardedHeadersOptions>(options => options.ForwardedHeaders = ForwardedHeaders.All);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseForwardedHeaders();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/minimal-sms", (string from) =>
{
    var response = new MessagingResponse();
    response.Message($"Ahoy {from}!");
    return Results.Extensions.TwiML(response, SaveOptions.DisableFormatting);
})
    .ValidateTwilioRequest();

app.MapPost("/minimal-sms", async (HttpRequest request) =>
    {
        var form = await request.ReadFormAsync();
        var from = form["from"];

        var response = new MessagingResponse();
        response.Message($"Ahoy {from}!");
        return Results.Extensions.TwiML(response);
    })
    .ValidateTwilioRequest();

app.Run();