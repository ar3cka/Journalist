using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Journalist.Collections;
using Journalist.EventStore.Notifications.Channels;
using Serilog;

namespace Journalist.EventStore.Notifications.Processing
{
    public class ReceivedNotificationProcessor : IReceivedNotificationProcessor
    {
        private static readonly ILogger s_logger = Log.ForContext<ReceivedNotificationProcessor>();

        private int m_processingCount;
        private INotificationHandler[] m_handlers;

        public ReceivedNotificationProcessor()
        {
            m_handlers = EmptyArray.Get<INotificationHandler>();
        }

        public void Process(IReceivedNotification notification)
        {
            Require.NotNull(notification, "notification");

            var processingTasks = new List<Task<bool>>();
            foreach (var notificationHandler in m_handlers)
            {
                Interlocked.Increment(ref m_processingCount);

                // run handling process in the threadpool
                var capturedHandler = notificationHandler;
                var processingTask = Task.Run(
                    () => capturedHandler.HandleNotificationAsync(notification.Notification));

                processingTasks.Add(processingTask.ContinueWith(handlingTask =>
                {
                    var hasError = false;
                    if (handlingTask.Exception != null)
                    {
                        s_logger.Fatal(
                            handlingTask.Exception.GetBaseException(),
                            "UNHANDLED EXCEPTION in NotificationListenerSubscription.");

                        hasError = true;
                    }

                    Interlocked.Decrement(ref m_processingCount);

                    return hasError;
                }));
            }

            Task.WhenAll(processingTasks)
                .ContinueWith(async resultTask =>
                {
                    try
                    {
                        if (resultTask.Result.Any(hasError => hasError))
                        {
                            await notification.RetryAsync();
                        }
                        else
                        {
                            await notification.CompleteAsync();
                        }
                    }
                    catch (Exception ex)
                    {
                        s_logger.Error(ex, "Notification completion failed.");
                    }
                });
        }

        public void RegisterHandlers(IEnumerable<INotificationHandler> handlers)
        {
            Require.NotNull(handlers, "handlers");

            m_handlers = handlers.ToArray();
        }

        public int ProcessingCount
        {
            get { return m_processingCount; }
        }
    }
}
