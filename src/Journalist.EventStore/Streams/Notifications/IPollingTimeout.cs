using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Streams.Notifications
{
    public interface IPollingTimeout
    {
        Task WaitAsync(CancellationToken token);

        void Reset();
    }
}
