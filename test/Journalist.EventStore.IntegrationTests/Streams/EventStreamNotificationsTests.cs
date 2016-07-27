using System;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Journalist.EventStore.Notifications.Types;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamNotificationsTests : IDisposable
    {
        public EventStreamNotificationsTests()
        {
            Listener1 = new NotificationListener1();
            Listener2 = new NotificationListener2();

            Connection = EventStoreConnectionBuilder
                .Create(config => config
                    .UseStorage(
                        storageConnectionString: "UseDevelopmentStorage=true",
                        journalTableName: "TestEventJournal",
                        notificationQueueName: "test-notification-queue-" + Guid.NewGuid().ToString("N"),
                        notificationQueuePartitionCount: 2)
                    .Notifications.EnableProcessing()
                    .Notifications.Subscribe(Listener1)
                    .Notifications.Subscribe(Listener2))
                .Build();

            StreamName = "notifications-tests-stream";
        }

        [Theory, AutoMoqData]
        public async Task Listeners_ReceivesSameStreamUpdatesNotifications(JournaledEvent[] events)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            await producer.PublishAsync(events);

            var item1 = TakeNotificationFromListener(Listener1);
            var item2 = TakeNotificationFromListener(Listener2);

            Assert.NotNull(item1);
            Assert.NotNull(item2);
            Assert.Equal(item1.NotificationId, item2.NotificationId);
        }

        public void Dispose()
        {
            Connection.Close();
        }

        private static EventStreamUpdated TakeNotificationFromListener(NotificationListener listener)
        {
            Assert.True(listener.Started);
            EventStreamUpdated item;
            listener.Notifications.TryTake(out item, TimeSpan.FromSeconds(30));

            return item;
        }

        public NotificationListener Listener1
        {
            get; private set;
        }

        public NotificationListener Listener2
        {
            get;
            private set;
        }

        public string StreamName
        {
            get; private set;
        }

        public IEventStoreConnection Connection
        {
            get; set;
        }
    }

    public class NotificationListener2 : NotificationListener
    {
    }

    public class NotificationListener1 : NotificationListener
    {
    }
}
