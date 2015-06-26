using Journalist.Collections;
using Journalist.EventSourced.Application.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore;
using Journalist.EventStore.Events;
using Journalist.EventStore.Streams;
using Moq;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;

namespace Journalist.EventSourced.Application.UnitTests.Infrastructure.Customizations
{
    public class EventStoreConnectionCustomization : ICustomization
    {
        private readonly bool m_hasEvents;

        public EventStoreConnectionCustomization(bool hasEvents)
        {
            m_hasEvents = hasEvents;
        }

        public void Customize(IFixture fixture)
        {
            fixture.Customize<Mock<IEventStoreConnection>>(composer => composer
                .Do(mock => mock
                    .Setup(self => self.CreateStreamReaderAsync(It.IsAny<string>()))
                    .ReturnsUsingFixture(fixture)));

            fixture.Customize<IEventStreamReader>(composer => composer
                .FromFactory((EventStreamReaderStub stub) => stub));

            if (!m_hasEvents)
            {
                fixture.Register(EmptyArray.Get<JournaledEvent>);
            }
        }
    }
}