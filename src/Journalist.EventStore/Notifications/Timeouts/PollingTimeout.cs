using System;
using System.Threading;
using System.Threading.Tasks;

namespace Journalist.EventStore.Notifications.Timeouts
{
    public class PollingTimeout : IPollingTimeout
    {
        private readonly double m_initialTimeout;
        private readonly double m_maximumTimout;
        private readonly double m_multiplier;
        private int m_callsCount;
        private TimeSpan m_value;

        public PollingTimeout() : this(TimeSpan.FromSeconds(1), 5.0, TimeSpan.FromSeconds(30))
        {
        }

        public PollingTimeout(TimeSpan initialTimeout, double multiplier, TimeSpan maximumTimout)
        {
            m_initialTimeout = initialTimeout.Seconds;
            m_multiplier = multiplier;
            m_maximumTimout = maximumTimout.Seconds;
            m_callsCount = 1;

            m_value = CalculateTimeoutValue(m_initialTimeout, m_multiplier, m_callsCount, m_maximumTimout);
        }

        public Task WaitAsync(CancellationToken token)
        {
            // Protect against TaskCanceledException.
            return Task.WhenAny(Task.Delay(m_value, token));
        }

        public void Increase()
        {
            m_callsCount = m_callsCount + 1;
            m_value = CalculateTimeoutValue(m_initialTimeout, m_multiplier, m_callsCount, m_maximumTimout);
        }

        public void Reset()
        {
            m_callsCount = 1;
            m_value = CalculateTimeoutValue(m_initialTimeout, m_multiplier, m_callsCount, m_maximumTimout);
        }

        private static TimeSpan CalculateTimeoutValue(double initialTimeout, double multiplier, int callsCount, double maximumTimout)
        {
            return TimeSpan.FromSeconds(Math.Min(initialTimeout * multiplier * callsCount, maximumTimout));
        }

        public TimeSpan Value
        {
            get { return m_value; }
        }
    }
}
