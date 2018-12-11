using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Journalist.EventStore.Utils.Polling
{
    public class PollingJob : IPollingJob
    {
        private static readonly ILogger s_logger = Log.ForContext<PollingJob>();
        private readonly IPollingTimeout m_timeout;
        private readonly string m_jobName;

        private Task m_pollingTask;
        private CancellationTokenSource m_cts;

        public PollingJob(string jobName, IPollingTimeout timeout)
        {
            Require.NotEmpty(jobName, nameof(jobName));
            Require.NotNull(timeout, nameof(jobName));

            m_jobName = jobName;
            m_timeout = timeout;
        }

        public void Start(PollingFunction func)
        {
            Require.NotNull(func, "func");

            m_cts = new CancellationTokenSource();
            m_pollingTask = PollingCycle(m_timeout, func, m_cts.Token);
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

        private async Task PollingCycle(IPollingTimeout timeout, PollingFunction func, CancellationToken token)
        {
            await Task.Yield();

            while (!token.IsCancellationRequested)
            {
                if (await func(token).ContinueWith(OnJobFunctionCompleted))
                {
                    timeout.Reset();
                    await timeout.WaitAsync(token);
                }
                else
                {
                    await timeout.WaitAsync(token);
                    timeout.Increase();
                }
            }
        }

        private bool OnJobFunctionCompleted(Task<bool> jobTask)
        {
            if (jobTask.Exception != null)
            {
                s_logger.Error(jobTask.Exception, "Polling job {JobName} function execution failed.", m_jobName);

                return false;
            }

            return jobTask.Result;
        }
    }
}
