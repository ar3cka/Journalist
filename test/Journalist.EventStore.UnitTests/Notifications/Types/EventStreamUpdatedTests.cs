using System;
using System.IO;
using System.Threading.Tasks;
using Journalist.EventStore.Journal;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Listeners;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Types
{
    public class EventStreamUpdatedTests
    {
        [Theory, AutoMoqData]
        public void IsAddressedTo_WhenNotificationWasNotAddressed_Throws(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            Assert.Throws<InvalidOperationException>(() => notification.IsAddressedTo(listener));
        }

        [Theory, AutoMoqData]
        public void IsAddressedTo_WhenRecipientSpecifiedDirectly_ReturnsTrue(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            Assert.True(notification
                .SendTo(listener)
                .IsAddressedTo(listener));
        }

        [Theory, AutoMoqData]
        public void AddressedNotification_WhenRecipientIsDifferent_ReturnsFalse(
            ListenerType1 listener1,
            ListenerType2 listener2,
            EventStreamUpdated notification,
            EventStreamReaderId originalConsumer,
            EventStreamReaderId sendedConsumer)
        {
            Assert.False(notification
                .SendTo(listener1)
                .IsAddressedTo(listener2));
        }

        [Theory, AutoMoqData]
        public void SendTo_PreservesNotificationPropertiesValues(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            var addressedNotification = (EventStreamUpdated)notification.SendTo(listener);

            Assert.Equal(notification.StreamName, addressedNotification.StreamName);
            Assert.Equal(notification.FromVersion, addressedNotification.FromVersion);
            Assert.Equal(notification.ToVersion, addressedNotification.ToVersion);
            Assert.Equal(notification.NotificationType, addressedNotification.NotificationType);
            Assert.Equal(notification.DeliveryCount, addressedNotification.DeliveryCount);
        }

        [Theory, AutoMoqData]
        public void SendTo_WhenMessagHasBeenAddressedToTheSameConsumer_ReturnsItself(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            var addressedNotification = notification.SendTo(listener);

            Assert.Same(addressedNotification, addressedNotification.SendTo(listener));
        }

        [Theory, AutoMoqData]
        public void SendTo_WhenMessagHasBeenAddressedToTheSameConsumer_SavesId(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            var addressedNotification = notification.SendTo(listener);

            Assert.Equal(
                addressedNotification.NotificationId,
                addressedNotification.SendTo(listener).NotificationId);
        }

        [Theory, AutoMoqData]
        public void SendTo_ChangesNotificationId(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            var addressedNotification = notification.SendTo(listener);

            Assert.NotEqual(notification.NotificationId, addressedNotification.NotificationId);
        }

        [Theory, AutoMoqData]
        public void RestoreFrom_IncreasesDeliveryCount(
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            Assert.Equal(0, notification.DeliveryCount);

            SaveAndRestore(notification);
            Assert.Equal(1, notification.DeliveryCount);

            SaveAndRestore(notification);
            Assert.Equal(2, notification.DeliveryCount);
        }

        [Theory, AutoMoqData]
        public void RedeliverTo_DecreasesDeliveryCount(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            SaveAndRestore(notification);

            Assert.Equal(0, notification.RedeliverTo(listener).DeliveryCount);
        }

        [Theory, AutoMoqData]
        public void RedeliverTo_WhenNotificationWasNotDelivered_DoesNotTouchDecreasesDeliveryCount(
            INotificationListener listener,
            EventStreamUpdated notification,
            EventStreamReaderId consumerId)
        {
            Assert.Equal(0, notification.RedeliverTo(listener).DeliveryCount);
        }

        private static void SaveAndRestore(INotification notification)
        {
            using (var memory = new MemoryStream())
            using (var writer = new StreamWriter(memory))
            using (var reader = new StreamReader(memory))
            {
                notification.SaveTo(writer);
                writer.Flush();
                memory.Position = 0;

                notification.RestoreFrom(reader);
            }
        }

        public class ListenerType1 : INotificationListener
        {
            public void OnSubscriptionStarted(INotificationListenerSubscription subscription)
            {
                throw new NotImplementedException();
            }

            public void OnSubscriptionStopped()
            {
                throw new NotImplementedException();
            }

            public Task On(EventStreamUpdated notification)
            {
                throw new NotImplementedException();
            }
        }

        public class ListenerType2 : INotificationListener
        {
            public void OnSubscriptionStarted(INotificationListenerSubscription subscription)
            {
                throw new NotImplementedException();
            }

            public void OnSubscriptionStopped()
            {
                throw new NotImplementedException();
            }

            public Task On(EventStreamUpdated notification)
            {
                throw new NotImplementedException();
            }
        }
    }
}
