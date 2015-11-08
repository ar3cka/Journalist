using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Utils.Polling
{
    public class PollingJob : IPollingJob
    {
        private readonly IPollingTimeout m_timeout;

        private Task m_pollingTask;
        private CancellationTokenSource m_cts;

        public PollingJob(IPollingTimeout timeout)
        {
            Require.NotNull(timeout, "timeout");

            m_timeout = timeout;
        }

        public void Start(PollingFunction func)
        {
            Require.NotNull(func, "func");

            m_cts = new CancellationTokenSource();
            m_pollingTask = PollingCycle(m_timeout, func, m_cts.Token);
        }

        private static async Task PollingCycle(IPollingTimeout timeout, PollingFunction func, CancellationToken token)
        {
            await Task.Yield();

            while (!token.IsCancellationRequested)
            {
                if (await func(token))
                {
                    timeout.Reset();
                }
                else
                {
                    await timeout.WaitAsync(token);
                    timeout.Increase();
                }
            }
        }

        public void Stop()
        {
            if (m_pollingTask != null)
            {
                m_cts.Cancel();
                m_pollingTask.Wait();
                m_pollingTask.Dispose();
                m_cts.Dispose();
            }
        }
    }
}
