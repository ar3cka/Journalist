using Journalist.EventStore.Streams;
using Journalist.Tasks;
using Moq;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.Customizations
{
    public class EventStreamConsumerMoqCustomization : ICustomization
    {
        private readonly bool m_consumerReceivingFailed;

        public EventStreamConsumerMoqCustomization(bool consumerReceivingFailed)
        {
            m_consumerReceivingFailed = consumerReceivingFailed;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<Mock<IEventStreamConsumer>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.ReceiveEventsAsync())
                    .Returns(() => m_consumerReceivingFailed ? TaskDone.False : TaskDone.True))
                .Do(mock => mock
                    .Setup(self => self.CloseAsync())
                    .Returns(TaskDone.Done))
                .Do(mock => mock
                    .Setup(self => self.CommitProcessedStreamVersionAsync(It.IsAny<bool>()))
                    .Returns(TaskDone.Done)));
        }
    }
}