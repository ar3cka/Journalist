using System;
using System.IO;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.Streams;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Types
{
    public class EventStreamUpdatedTests
    {
        [Theory, AutoMoqData]
        public void IsAddressedTo_WhenNotificationWasNotAddressed_Throws(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            Assert.Throws<InvalidOperationException>(() => notification.IsAddressedTo(consumerId));
        }

        [Theory, AutoMoqData]
        public void IsAddressedTo_WhenRecipientSpecifiedDirectly_ReturnsTrue(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            Assert.True(notification
                .SendTo(consumerId)
                .IsAddressedTo(consumerId));
        }

        [Theory, AutoMoqData]
        public void AddressedNotification_WhenRecipientIsDifferent_ReturnsFalse(
            EventStreamUpdated notification,
            EventStreamConsumerId originalConsumer,
            EventStreamConsumerId sendedConsumer)
        {
            Assert.False(notification
                .SendTo(originalConsumer)
                .IsAddressedTo(sendedConsumer));
        }

        [Theory, AutoMoqData]
        public void SendTo_PreservesNotificationPropertiesValues(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            var addressedNotification = (EventStreamUpdated)notification.SendTo(consumerId);

            Assert.Equal(notification.StreamName, addressedNotification.StreamName);
            Assert.Equal(notification.FromVersion, addressedNotification.FromVersion);
            Assert.Equal(notification.ToVersion, addressedNotification.ToVersion);
            Assert.Equal(notification.NotificationType, addressedNotification.NotificationType);
            Assert.Equal(notification.DeliveryCount, addressedNotification.DeliveryCount);
        }

        [Theory, AutoMoqData]
        public void SendTo_WhenMessagHasBeenAddressedToTheSameConsumer_ReturnsItself(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            var addressedNotification = notification.SendTo(consumerId);

            Assert.Same(addressedNotification, addressedNotification.SendTo(consumerId));
        }

        [Theory, AutoMoqData]
        public void SendTo_WhenMessagHasBeenAddressedToTheSameConsumer_SavesId(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            var addressedNotification = notification.SendTo(consumerId);

            Assert.Equal(
                addressedNotification.NotificationId,
                addressedNotification.SendTo(consumerId).NotificationId);
        }

        [Theory, AutoMoqData]
        public void SendTo_ChangesNotificationId(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            var addressedNotification = notification.SendTo(consumerId);

            Assert.NotEqual(notification.NotificationId, addressedNotification.NotificationId);
        }

        [Theory, AutoMoqData]
        public void RestoreFrom_IncreasesDeliveryCount(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            Assert.Equal(0, notification.DeliveryCount);

            SaveAndRestore(notification);
            Assert.Equal(1, notification.DeliveryCount);

            SaveAndRestore(notification);
            Assert.Equal(2, notification.DeliveryCount);
        }

        [Theory, AutoMoqData]
        public void RedeliverTo_DecreasesDeliveryCount(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            SaveAndRestore(notification);

            Assert.Equal(0, notification.RedeliverTo(consumerId).DeliveryCount);
        }

        [Theory, AutoMoqData]
        public void RedeliverTo_WhenNotificationWasNotDelivered_DoesNotTouchDecreasesDeliveryCount(
            EventStreamUpdated notification,
            EventStreamConsumerId consumerId)
        {
            Assert.Equal(0, notification.RedeliverTo(consumerId).DeliveryCount);
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
    }
}
