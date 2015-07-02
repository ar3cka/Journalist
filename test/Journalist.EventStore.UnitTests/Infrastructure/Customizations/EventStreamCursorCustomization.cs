using System.Collections.Generic;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.Tasks;
using Ploeh.AutoFixture;

namespace Journalist.EventStore.UnitTests.Infrastructure.Customizations
{
    public class EventStreamCursorCustomization : ICustomization
    {
        private readonly bool m_emptyCursor;

        public EventStreamCursorCustomization(bool emptyCursor)
        {
            m_emptyCursor = emptyCursor;
        }

        public void Customize(IFixture fixture)
        {
            if (m_emptyCursor)
            {
                fixture.Customize<IEventStreamCursor>(composer => composer
                    .FromFactory(() => EventStreamCursor.Empty));
            }
            else
            {
                fixture.Customize<SortedList<StreamVersion, JournaledEvent>>(composer => composer
                    .FromFactory(() => new SortedList<StreamVersion, JournaledEvent>
                    {
                        { StreamVersion.Create(1), fixture.Create<JournaledEvent>() },
                        { StreamVersion.Create(2), fixture.Create<JournaledEvent>() },
                        { StreamVersion.Create(3), fixture.Create<JournaledEvent>() }
                    }));

                fixture.Customize<FetchEvents>(composer => composer
                    .FromFactory(
                        () => version => fixture.Create<SortedList<StreamVersion, JournaledEvent>>().YieldTask()));

                fixture.Customize<EventStreamPosition>(composer => composer
                    .FromFactory(() => new EventStreamPosition(
                        fixture.Create("ETag"),
                        StreamVersion.Create(3))));

                fixture.Customize<IEventStreamCursor>(composer => composer
                    .FromFactory(() => new EventStreamCursor(
                        fixture.Create<EventStreamPosition>(),
                        StreamVersion.Start,
                        fixture.Create<FetchEvents>())));
            }
        }
    }
}