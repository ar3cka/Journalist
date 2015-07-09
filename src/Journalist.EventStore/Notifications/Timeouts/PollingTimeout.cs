using System;
using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Timeouts
{
    public class PollingTimeout : IPollingTimeout
    {
        public Task WaitAsync(CancellationToken token)
        {
            // Protect against TaskCanceledException.
            return Task.WhenAny(Task.Delay(TimeSpan.FromSeconds(10), token));
        }

        public void Reset()
        {
        }
    }
}
