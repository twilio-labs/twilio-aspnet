using System.Net;
using Microsoft.AspNetCore.Http;

namespace Twilio.AspNet.Core.UnitTests;

public class ContextMocks
{
    public Moq.Mock<HttpContext> HttpContext { get; set; }
    public Moq.Mock<HttpRequest> Request { get; set; }

    public ContextMocks(bool isLocal, FormCollection? form = null, bool isProxied = false) : this("", isLocal, form,
        isProxied)
    {
    }

    public ContextMocks(string urlOverride, bool isLocal, FormCollection? form = null, bool isProxied = false)
    {
        var headers = new HeaderDictionary();
        headers.Add("X-Twilio-Signature", CalculateSignature(urlOverride, form));
        if (isProxied)
        {
            headers.Add("X-Forwarded-For", "1.1.1.1");
        }

        var connectionInfo = new Moq.Mock<ConnectionInfo>();
        connectionInfo.Setup(x => x.RemoteIpAddress).Returns(isLocal ? IPAddress.Loopback : IPAddress.Parse("1.1.1.1"));

        HttpContext = new Moq.Mock<HttpContext>();
        Request = new Moq.Mock<HttpRequest>();
        HttpContext.Setup(x => x.Request).Returns(Request.Object);
        HttpContext.Setup(x => x.Connection).Returns(connectionInfo.Object);
        Request.Setup(x => x.Headers).Returns(headers);
        Request.Setup(x => x.HttpContext).Returns(HttpContext.Object);

        var uri = new Uri(FakeUrl);
        Request.Setup(x => x.QueryString).Returns(new QueryString(uri.Query));
        Request.Setup(x => x.Scheme).Returns(uri.Scheme);
        Request.Setup(x => x.Host).Returns(new HostString(uri.Host));
        Request.Setup(x => x.Path).Returns(new PathString(uri.AbsolutePath));

        if (form is null) return;
        Request.Setup(x => x.Method).Returns("POST");
        Request.Setup(x => x.Form).Returns(form);
        Request.Setup(x => x.ReadFormAsync(new CancellationToken()))
            .Returns(() => Task.FromResult<IFormCollection>(form));
        Request.Setup(x => x.HasFormContentType).Returns(true);
    }

    public const string FakeUrl = "https://api.example.com/webhook";
    public const string FakeAuthToken = "thisisafakeauthtoken";

    private static string CalculateSignature(string? urlOverride, FormCollection? form)
        => ValidationHelper.CalculateSignature(
            string.IsNullOrEmpty(urlOverride) ? FakeUrl : urlOverride,
            FakeAuthToken,
            form
        );
}