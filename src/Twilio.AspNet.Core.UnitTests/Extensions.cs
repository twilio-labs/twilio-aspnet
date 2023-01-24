using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Twilio.AspNet.Core.UnitTests;

public static class Extensions
{
    public static Task<T> WaitForOptionChange<T>(this IOptionsMonitor<T> monitor)
    {
        var tcs = new TaskCompletionSource<T>();
        var cts = new CancellationTokenSource();
        IDisposable disposable = null;
        disposable = monitor.OnChange(options =>
        {
            disposable.Dispose();
            cts.Cancel();
            tcs.SetResult(options);
        });

        Task.Delay(1000, cts.Token)
            .ContinueWith(_ =>
            {
                disposable.Dispose();
                tcs.SetException(new Exception("WaitForOptionChange timed out."));
            }, cts.Token);
        
        return tcs.Task;
    }
}