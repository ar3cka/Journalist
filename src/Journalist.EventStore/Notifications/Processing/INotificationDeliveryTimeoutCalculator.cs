using System;

namespace Journalist.EventStore.Notifications.Processing
{
    public interface INotificationDeliveryTimeoutCalculator
    {
        TimeSpan CalculateDeliveryTimeout(int deliveryCount);
    }
}
