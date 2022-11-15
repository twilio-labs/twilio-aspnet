using Twilio.AspNet.Core;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services
    .AddTwilioClient()
    .AddTwilioRequestValidation();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/minimal-sms", (string from) =>
{
    var response = new MessagingResponse();
    response.Message($"Ahoy {from}!");
    return Results.Extensions.TwiML(response);
});

app.MapPost("/minimal-sms", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var from = form["from"];
    
    var response = new MessagingResponse();
    response.Message($"Ahoy {from}!");
    return Results.Extensions.TwiML(response);
});

app.Run();