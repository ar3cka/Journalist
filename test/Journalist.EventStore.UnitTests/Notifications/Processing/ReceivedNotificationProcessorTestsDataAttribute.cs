using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;

namespace Journalist.EventStore.UnitTests.Notifications.Processing
{
    public class ReceivedNotificationProcessorTestsDataAttribute : AutoMoqDataAttribute
    {
        public ReceivedNotificationProcessorTestsDataAttribute()
        {
            Fixture.Customize<NotificationHandlerStub>(composer => composer
                .FromFactory(() => new NotificationHandlerStub(ThrowOnNotificationHandling)));
        }

        public bool ThrowOnNotificationHandling { get; set; }
    }
}
