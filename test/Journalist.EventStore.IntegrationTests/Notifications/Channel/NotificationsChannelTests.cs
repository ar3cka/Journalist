using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Types;
using Journalist.WindowsAzure.Storage;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Notifications.Channel
{
    public class NotificationsChannelTests
    {
        private readonly INotificationsChannel m_channel;
        private readonly IFixture m_fixture;

        public NotificationsChannelTests()
        {
            var storage = new StorageFactory();

            var queues = Enumerable
                .Range(0, 16)
                .Select(index => storage.CreateQueue("UseDevelopmentStorage=true", "notifications-channel-tests-" + index.ToString("D1")))
                .ToArray();

            m_channel = new NotificationsChannel(queues, new NotificationFormatter());

            m_fixture = new Fixture().Customize(new AutoConfiguredMoqCustomization());
            m_fixture.RepeatCount = 16 * 2;
        }

        [Fact]
        public async Task ChannelReceivesAllSendedNotifications()
        {
            var notifications = m_fixture.CreateMany<EventStreamUpdated>();
            foreach (var notification in notifications)
            {
                await m_channel.SendAsync(notification);
            }

            var receivedNotification = new List<INotification>();
            foreach (var _ in Enumerable.Range(0, notifications.Count()))
            {
                receivedNotification.AddRange(await m_channel.ReceiveNotificationsAsync());
            }

            Assert.Equal(notifications.Count(), receivedNotification.Count);
        }
    }
}
