using System;

namespace Journalist.EventStore.Notifications.Processing
{
    public sealed class NotificationDeliveryTimeoutCalculator : INotificationDeliveryTimeoutCalculator
    {
        public TimeSpan CalculateDeliveryTimeout(int deliveryCount)
        {
            Require.Positive(deliveryCount, nameof(deliveryCount));

            if (deliveryCount <= Constants.Settings.MAX_NOTIFICATION_PROCESSING_LINEAR_RETRY_ATTEMPT_COUNT)
            {
                return TimeSpan.FromSeconds(
                    deliveryCount * Constants.Settings.NOTIFICATION_RETRY_DELIVERY_TIMEOUT_MULTIPLYER_SEC);
            }

            var exponentialAttempts = deliveryCount - Constants.Settings.MAX_NOTIFICATION_PROCESSING_LINEAR_RETRY_ATTEMPT_COUNT;

            if (exponentialAttempts >= Constants.Settings.MAX_NOTIFICATION_PROCESSING_EXPONENTIAL_RETRY_ATTEMPT_COUNT) // no matter to calculate the delay + overflow every 32 attempts
            {
                return TimeSpan.FromMinutes(MAX_TIMEOUT_IN_MINUTES);
            }

            return TimeSpan.FromMinutes(((1 << exponentialAttempts) - 1.0) / 2.0);
        }

        private const double MAX_TIMEOUT_IN_MINUTES = 60.0;
    }
}
