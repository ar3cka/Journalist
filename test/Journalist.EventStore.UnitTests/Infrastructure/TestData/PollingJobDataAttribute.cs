using System;
using System.Threading.Tasks;
using Journalist.EventStore.Utils.Polling;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class PollingJobDataAttribute : AutoMoqDataAttribute
    {
        public PollingJobDataAttribute(bool successfulPoll = true, bool fail = false)
        {
            var failedTcs = new TaskCompletionSource<bool>();
            failedTcs.SetException(new InvalidOperationException());

            Fixture.Customize<PollingFunction>(composer =>
                composer.FromFactory(() => fail
                    ? (PollingFunction)(token => failedTcs.Task)
                    : (token => successfulPoll.YieldTask())));
        }
    }
}
