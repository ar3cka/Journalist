using System;
using Journalist.EventStore.Notifications.Processing;
using Journalist.EventStore.UnitTests.Infrastructure.TestData;
using Xunit;

namespace Journalist.EventStore.UnitTests.Notifications.Processing
{
    public class NotificationDeliveryTimeoutCalculatorTests
    {
        [Theory, AutoMoqData]
        public void CalculateDeliveryTimeout_WhenLinearAttempts_RetunsLinearTimeout(NotificationDeliveryTimeoutCalculator calculator)
        {
            var deliveryAttempts = 5;
            var timeout = TimeSpan.FromSeconds(deliveryAttempts * 2);

            var result = calculator.CalculateDeliveryTimeout(deliveryAttempts);

            Assert.Equal(timeout, result);
        }

        [Theory, AutoMoqData]
        public void CalculateDeliveryTimeout_WhenAttemptsGreaterThanSixteen_RetunsAnHour(NotificationDeliveryTimeoutCalculator calculator)
        {
            var deliveryAttempts = 17;
            var hour = TimeSpan.FromHours(1);

            var result = calculator.CalculateDeliveryTimeout(deliveryAttempts);

            Assert.Equal(hour, result);
        }
    }
}
