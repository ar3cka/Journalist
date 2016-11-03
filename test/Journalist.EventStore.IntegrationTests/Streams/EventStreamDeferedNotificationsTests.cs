using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journalist.EventStore.Connection;
using Journalist.EventStore.Events;
using Journalist.EventStore.IntegrationTests.Infrastructure.TestData;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.Tasks;
using Xunit;

namespace Journalist.EventStore.IntegrationTests.Streams
{
    public class EventStreamDeferedNotificationsTests
    {
        public EventStreamDeferedNotificationsTests()
        {
            BatchEventListener = new BatchEventListener();
            EventListener = new EventListener();

            var queueName = Guid.NewGuid().ToString("N");
            StreamName = Guid.NewGuid().ToString("N");

            Connection = EventStoreConnectionBuilder
                .Create(config => config
                    .UseStorage(
                        storageConnectionString: "UseDevelopmentStorage=true",
                        journalTableName: "TestEventJournal" + Guid.NewGuid().ToString("N"),
                        notificationQueuePartitionCount: 1,
                        notificationQueueName: queueName)
                    .Notifications.EnableProcessing()
                    .Notifications.Subscribe(BatchEventListener)
                    .Notifications.Subscribe(EventListener))
                .Build();
        }

        [Theory, AutoMoqData]
        public async Task Listeners_ReceivesSameStreamUpdatesNotifications(JournaledEvent[] events)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);
            BatchEventListener.ExpectedEventCount = events.Length;
            EventListener.ExpectedEventCount = events.Length;

            await producer.PublishAsync(events);

            var item1 = TakeNotificationFromListener(EventListener, events.Length);
            var item2 = TakeNotificationFromListener(BatchEventListener, events.Length);

            Assert.NotNull(item1);
            Assert.NotNull(item2);
            Assert.Equal(events, item1);
            Assert.Equal(events, item2);
        }

        [Theory, AutoMoqData]
        public async Task Listeners_WithDelay_ReceivesSameStreamUpdatesNotifications(JournaledEvent[] events1, JournaledEvent[] events2)
        {
            var producer = await Connection.CreateStreamProducerAsync(StreamName);

            var expectedEvents = events1.Union(events2).ToArray();
            BatchEventListener.ExpectedEventCount = expectedEvents.Length;
            EventListener.ExpectedEventCount = expectedEvents.Length;

            await producer.PublishAsync(events1);
            await Task.Delay(TimeSpan.FromSeconds(5));
            await producer.PublishAsync(events2);

            var item1 = TakeNotificationFromListener(EventListener, expectedEvents.Length);
            var item2 = TakeNotificationFromListener(BatchEventListener, expectedEvents.Length);

            Assert.NotNull(item1);
            Assert.NotNull(item2);
            Assert.Equal(expectedEvents, item1);
            Assert.Equal(expectedEvents, item2);
        }

        private static List<JournaledEvent> TakeNotificationFromListener(EventListener listener, int numberEvents)
        {
            var events = new List<JournaledEvent>();
            foreach (var journaledEvent in listener.Events.GetConsumingEnumerable())
            {
                events.Add(journaledEvent);
            }

            return events;
        }

        private static List<JournaledEvent> TakeNotificationFromListener(BatchEventListener listener, int numberEvents)
        {
            var events = new List<JournaledEvent>();
            foreach (var journaledEvent in listener.Events.GetConsumingEnumerable())
            {
                events.Add(journaledEvent);
            }

            return events;
        }

        public void Dispose()
        {
            Connection.Close();
        }

        public BatchEventListener BatchEventListener
        {
            get;
            private set;
        }

        public EventListener EventListener
        {
            get;
            private set;
        }

        public string StreamName
        {
            get;
            private set;
        }

        public IEventStoreConnection Connection
        {
            get;
            set;
        }
    }

    public class EventListener : EventConsumingNotificationListener
    {
        public readonly BlockingCollection<JournaledEvent> Events = new BlockingCollection<JournaledEvent>(new ConcurrentQueue<JournaledEvent>());

        private bool m_throwException = true;
        private int m_eventCount;

        protected override Task ProcessEventAsync(JournaledEvent journaledEvent)
        {
            if (m_throwException)
            {
                m_throwException = false;
                throw new Exception();
            }

            Events.Add(journaledEvent);

            if (++m_eventCount == ExpectedEventCount)
            {
                Events.CompleteAdding();
            }

            return TaskDone.Done;
        }

        public int ExpectedEventCount { get; set; }
    }

    public class BatchEventListener : BatchEventConsumingNotificationListener
    {
        public readonly BlockingCollection<JournaledEvent> Events = new BlockingCollection<JournaledEvent>(new ConcurrentQueue<JournaledEvent>());

        private bool m_throwException = true;
        private int m_eventCount;

        protected override Task ProcessEventBatchAsync(JournaledEvent[] journaledEvent)
        {
            if (m_throwException)
            {
                m_throwException = false;
                throw new Exception();
            }

            foreach (var e in journaledEvent)
            {
                Events.Add(e);
            }

            m_eventCount += journaledEvent.Length;
            if (m_eventCount == ExpectedEventCount)
            {
                Events.CompleteAdding();
            }

            return TaskDone.Done;
        }

        public int ExpectedEventCount { get; set; }
    }
}
