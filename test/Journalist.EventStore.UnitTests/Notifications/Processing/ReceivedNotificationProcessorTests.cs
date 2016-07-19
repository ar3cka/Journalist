using System;
using System.Linq;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Notifications;
using Journalist.EventStore.Notifications.Channels;
using Journalist.EventStore.Notifications.Processing;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Moq;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Processing
{
    public class ReceivedNotificationProcessorTests
    {
        [Theory(Skip = "Unstable test"), AutoMoqData]
        public void Process_SendsNotificationToHandlers(
            IReceivedNotification notification,
            INotificationHandler[] handlers,
            ReceivedNotificationProcessor processor)
        {
            processor.RegisterHandlers(handlers);

            processor.Process(notification);

            foreach (var handlerMock in handlers.Select(Mock.Get))
            {
                handlerMock.Verify(self => self.HandleNotificationAsync(notification.Notification));
            }
        }

        [Theory, AutoMoqData]
        public async Task Process_CompletesNotification(
            Mock<IReceivedNotification> notificationMock,
            INotificationHandler[] handlers,
            ReceivedNotificationProcessor processor)
        {
            processor.RegisterHandlers(handlers);

            processor.Process(notificationMock.Object);

            await WaitProcessingCompletion(processor);

            notificationMock
                .Verify(self => self.CompleteAsync());
        }

        [Theory, AutoMoqData]
        public async Task Process_WhenHandlerFailes_RetriesNotification(
            Mock<IReceivedNotification> notificationMock,
            Mock<INotificationHandler> handlerMock,
            ReceivedNotificationProcessor processor)
        {
            handlerMock
                .Setup(self => self.HandleNotificationAsync(It.IsAny<INotification>()))
                .Throws<NotImplementedException>();

            processor.RegisterHandlers(handlerMock.Object.YieldArray());

            processor.Process(notificationMock.Object);

            await WaitProcessingCompletion(processor);

            notificationMock
                .Verify(self => self.RetryAsync());
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
