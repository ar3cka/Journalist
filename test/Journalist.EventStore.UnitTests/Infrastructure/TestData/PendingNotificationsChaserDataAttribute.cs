using System;
using System.Collections.Generic;
using Journalist.Collections;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Types;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Journalist.EventStore.Utils.Polling;
using Journalist.WindowsAzure.Storage.Blobs;
using Moq;

namespace Journalist.EventStore.UnitTests.Infrastructure.TestData
{
    public class PendingNotificationsChaserDataAttribute : AutoMoqDataAttribute
    {
        public PendingNotificationsChaserDataAttribute(bool lockWasNotAcquired = false, bool noNotifications = false)
        {
            Fixture.Customize<IPollingJob>(composer => composer.FromFactory((PollingJobStub stub) => stub));
            Fixture.Customize<PendingNotificationsChaser>(composer => composer.Do(chaser => chaser.Start()));

            if (lockWasNotAcquired)
            {
                Fixture.Customize<Mock<ICloudBlockBlob>>(composer => composer
                    .Do(mock => mock
                        .Setup(self => self.AcquireLeaseAsync(It.IsAny<TimeSpan?>()))
                        .Throws<LeaseAlreadyAcquiredException>()));
            }

            if (noNotifications)
            {
                Fixture.Customize<IReadOnlyList<EventStreamUpdated>>(composer => composer
                    .FromFactory(EmptyArray.Get<EventStreamUpdated>));
            }
            else
            {
                Fixture.Customize<IReadOnlyList<EventStreamUpdated>>(composer => composer
                    .FromFactory((EventStreamUpdated[] notifications) => notifications));
            }
        }
    }
}