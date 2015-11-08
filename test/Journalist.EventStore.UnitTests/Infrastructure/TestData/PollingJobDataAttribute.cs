using Journalist.EventStore.UnitTests.Utils.Polling;
using Journalist.EventStore.Utils.Polling;
using Journalist.Tasks;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class PollingJobDataAttribute : AutoMoqDataAttribute
    {
        public PollingJobDataAttribute(bool successfulPoll = true)
        {
            Fixture.Customize<PollingFunction>(composer =>
                composer.FromFactory(() => token => successfulPoll.YieldTask()));
        }
    }
}