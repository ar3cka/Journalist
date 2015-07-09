using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Timeouts
{
    public interface IPollingTimeout
    {
        Task WaitAsync(CancellationToken token);

        void Reset();
    }
}
