using System;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Notifications.Processing;
using Journalist.EventStore.UnitTests.Infrastructure.Stubs;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Processing
{
    public class ReceivedNotificationProcessorTests
    {
        [Theory, ReceivedNotificationProcessorTestsData]
        public async Task Process_SendsNotificationToHandlers(
            ReceivedNotificationStub notification,
            NotificationHandlerStub[] handlers,
            ReceivedNotificationProcessor processor)
        {
            processor.RegisterHandlers(handlers);

            await ProcessNotifications(notification, processor);

            foreach (var handler in handlers)
            {
                Assert.True(handler.ReceivedNotifications.Contains(notification.Notification));
            }
        }

        [Theory, ReceivedNotificationProcessorTestsData]
        public async Task Process_CompletesNotification(
            ReceivedNotificationStub notification,
            NotificationHandlerStub[] handlers,
            ReceivedNotificationProcessor processor)
        {
            processor.RegisterHandlers(handlers);

            await ProcessNotifications(notification, processor);

            Assert.True(notification.IsCompleted);
        }

        [Theory, ReceivedNotificationProcessorTestsData(ThrowOnNotificationHandling = true)]
        public async Task Process_WhenHandlerFailes_RetriesNotification(
            ReceivedNotificationStub notification,
            NotificationHandlerStub handler,
            ReceivedNotificationProcessor processor)
        {
            processor.RegisterHandlers(handler.YieldArray());

            await ProcessNotifications(notification, processor);

            Assert.True(notification.IsRetried);
        }

        private static async Task ProcessNotifications(ReceivedNotificationStub notification, ReceivedNotificationProcessor processor)
        {
            processor.Process(notification);

            await WaitProcessingCompletion(processor);
        }

        private static async Task WaitProcessingCompletion(IReceivedNotificationProcessor processor)
        {
            while (processor.ProcessingCount != 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(5));
            }

            // wait notification completion
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
