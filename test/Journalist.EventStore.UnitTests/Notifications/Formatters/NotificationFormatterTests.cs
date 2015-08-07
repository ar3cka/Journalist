using System.IO;
using Journalist.EventStore.Notifications.Formatters;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Journalist.EventStore.UnitTests.Notifications.Formatters.Templates;
using Journalist.Extensions;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Formatters
{
    public class NotificationFormatterTests
    {
        [Theory]
        [AutoMoqData]
        public void ToBytes_ForEventStreamUpdatedTest(NotificationFormatter formatter, EventStreamUpdated notification)
        {
            var bytes = formatter.ToBytes(notification);

            Assert.Equal(
                TemplatesRes.EventStreamUpdated.FormatString(
                    notification.NotificationId.ToString("N"),
                    notification.StreamName,
                    (int)notification.FromVersion,
                    (int)notification.ToVersion),
                ReadBytes(bytes));
        }

        [Theory]
        [AutoMoqData]
        public void FromBytes_ForEventStreamUpdatedTest(NotificationFormatter formatter, EventStreamUpdated notification)
        {
            var bytes = formatter.ToBytes(notification);
            var restoredNotification = (EventStreamUpdated)formatter.FromBytes(bytes);

            Assert.Equal(notification.NotificationId, restoredNotification.NotificationId);
            Assert.Equal(notification.NotificationType, restoredNotification.NotificationType);
            Assert.Equal(notification.StreamName, restoredNotification.StreamName);
            Assert.Equal(notification.FromVersion, restoredNotification.FromVersion);
            Assert.Equal(notification.ToVersion, restoredNotification.ToVersion);
        }

        private static string ReadBytes(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
