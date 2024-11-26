using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Twilio.AspNet.Core.UnitTests;

internal static class ValidationHelper
{
    internal static string CalculateSignature(string url, string authToken, IFormCollection? form)
    {
        var value = new StringBuilder(url);
        if (form is not null)
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