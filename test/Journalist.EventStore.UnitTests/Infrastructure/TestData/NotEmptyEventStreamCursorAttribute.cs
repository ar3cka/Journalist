using System.Collections.Generic;
using Journalist.EventStore.Events;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Journal.StreamCursor;
using Journalist.EventStore.UnitTests.Infrastructure.Customizations.Customizations;
using Journalist.Tasks;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Ploeh.AutoFixture.Xunit2;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class NotEmptyEventStreamCursorAttribute : AutoDataAttribute
    {
        public NotEmptyEventStreamCursorAttribute()
        {
            Fixture.Customize(new AutoConfiguredMoqCustomization());
            Fixture.Customize(new JournaledEventCustomization());

            Fixture.Customize<SortedList<StreamVersion, JournaledEvent>>(composer => composer
                .FromFactory(() => new SortedList<StreamVersion, JournaledEvent>
                {
                    { StreamVersion.Create(1), Fixture.Create<JournaledEvent>() },
                    { StreamVersion.Create(2), Fixture.Create<JournaledEvent>() },
                    { StreamVersion.Create(3), Fixture.Create<JournaledEvent>() }
                }));

            Fixture.Customize<FetchEvents>(composer => composer
                .FromFactory(() => version => Fixture.Create<SortedList<StreamVersion, JournaledEvent>>().YieldTask()));

            Fixture.Customize<EventStreamPosition>(composer => composer
                .FromFactory(() => new EventStreamPosition(
                    Fixture.Create("ETag"),
                    StreamVersion.Create(3))));

            Fixture.Customize<EventStreamCursor>(composer => composer
                .FromFactory(() => new EventStreamCursor(
                    Fixture.Create<EventStreamPosition>(),
                    StreamVersion.Start,
                    Fixture.Create<FetchEvents>())));
        }
    }
}
