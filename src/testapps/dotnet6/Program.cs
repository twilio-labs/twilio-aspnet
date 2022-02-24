using Twilio.AspNet.Core.MinimalApi;
using Twilio.TwiML;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapMethods("/minimal-sms", new[] {"get", "post"}, () =>
{
    var messagingResponse = new MessagingResponse();
    messagingResponse.Message("The Robots are coming! Head for the hills!!");

    return Results.Extensions.TwiML(messagingResponse);
});

app.Run();