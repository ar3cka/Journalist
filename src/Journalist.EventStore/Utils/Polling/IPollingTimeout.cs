using System;
using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Utils.Polling
{
    public interface IPollingTimeout
    {
        Task WaitAsync(CancellationToken token);

        void Reset();

        void Increase();

        TimeSpan Value { get; }
    }
}
