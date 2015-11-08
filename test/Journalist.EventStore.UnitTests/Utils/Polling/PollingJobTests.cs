using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.EventStore.Utils.Polling;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;

namespace Journalist.EventStore.UnitTests.Utils.Polling
{
    public class PollingJobTests
    {
        [Theory, PollingJobData]
        public async Task StartedJob_WhenFuncReturnsTrue_ResetsTimeout(
            [Frozen] Mock<IPollingTimeout> timeout,
            [Frozen] PollingFunction pollingFunc,
            PollingJob pollingJob)
        {
            await RunWaitStopJob(pollingJob, pollingFunc);

            timeout.Verify(self => self.Reset(), Times.AtLeastOnce());
        }

        [Theory, PollingJobData(successfulPoll: false)]
        public async Task StartedJob_WhenFuncReturnsFalse_Waits(
            [Frozen] Mock<IPollingTimeout> timeout,
            [Frozen] PollingFunction pollingFunc,
            PollingJob pollingJob)
        {
            await RunWaitStopJob(pollingJob, pollingFunc);

            timeout.Verify(self => self.WaitAsync(It.IsAny<CancellationToken>()), Times.AtLeastOnce());
        }

        [Theory, PollingJobData(successfulPoll: false)]
        public async Task StartedJob_WhenFuncReturnsFalse_IncreasesTimeout(
            [Frozen] Mock<IPollingTimeout> timeout,
            [Frozen] PollingFunction pollingFunc,
            PollingJob pollingJob)
        {
            await RunWaitStopJob(pollingJob, pollingFunc);

            timeout.Verify(self => self.Increase(), Times.AtLeastOnce());
        }

        private static async Task RunWaitStopJob(PollingJob pollingJob, PollingFunction polllingFunc)
        {
            pollingJob.Start(polllingFunc);
            await Task.Delay(TimeSpan.FromSeconds(1));
            pollingJob.Stop();
        }
    }
}
