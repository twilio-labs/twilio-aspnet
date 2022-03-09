using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Net.Mime;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis;
using Twilio.AspNet.Common;

// ReSharper disable once CheckNamespace
namespace Twilio.AspNet.Core.MinimalApi;

/// <summary>
/// Adds extension methods to Results.Extensions to write TwiML objects to the HTTP response body
/// </summary>
public partial class TwilioRequestBinding<T> where T : TwilioRequest
{
    public T BindingResult { get; set; }

    public static ValueTask<TwilioRequestBinding<T>> BindAsync(HttpContext context)
    {
        var request = context.Request;
        var t = Activator.CreateInstance<T>();
        switch (request.Method)
        {
            case "POST":
                BindFromForm(request.Form, t);
                break;
            case "GET":
                break;
            default:
                throw new Exception("TwilioRequestBinding only supports POST/GET requests");
        }

        return ValueTask.FromResult<TwilioRequestBinding<T>>(new TwilioRequestBinding<T>
        {
            BindingResult = t
        });
    }

    static partial void BindFromForm(IFormCollection collection, T t);
}