using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Utils.Polling
{
    public delegate Task<bool> PollingFunction(CancellationToken cancellationToken);

    public interface IPollingJob
    {
        void Start(PollingFunction func);

        void Stop();
    }
}
