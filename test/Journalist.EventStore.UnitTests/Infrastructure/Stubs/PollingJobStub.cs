using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.Utils.Polling;

namespace Journalist.EventStore.UnitTests.Infrastructure.Stubs
{
    public class PollingJobStub : IPollingJob
    {
        private PollingFunction m_pollingFunc;

        public void Start(PollingFunction func)
        {
            m_pollingFunc = func;

            JobStarted = true;
            JobStoped = false;
        }

        public void Stop()
        {
            JobStarted = false;
            JobStoped = true;

            m_pollingFunc = null;
        }

        public async Task<bool> Poll()
        {
            Ensure.True(JobStarted, "Job was not started");

            return await m_pollingFunc(CancellationToken.None);
        }

        public bool JobStarted { get; private set; }

        public bool JobStoped { get; private set; }
    }
}
