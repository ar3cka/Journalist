using Journalist.EventStore.Streams;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.Customizations
{
    public class EventStreamConsumingSessionCustomization : ICustomization
    {
        public EventStreamConsumingSessionCustomization(bool leaderPromotion)
        {
            LeaderPromotion = leaderPromotion;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<Mock<IEventStreamConsumingSession>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.FreeAsync())
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.PromoteToLeaderAsync())
                    .Returns(LeaderPromotion ? TaskDone.True : TaskDone.False)));
        }

        public bool LeaderPromotion { get; private set; }
    }
}
