using System;
using System.Threading;
using System.Threading.Tasks;
using Journalist.Extensions;

namespace Journalist.EventStore.Notifications.Timeouts
{
    public class PollingTimeout : IPollingTimeout
    {
        public const int INCREASING_THRESHOLD = 10;
        public const int INITIAL_TIMEOUT_SEC = 1;
        public const double TIMEOUT_MULTIPLIER = 5.0;
        public const int MAX_TIMEOUT_SEC = 30;

        private readonly double m_initialTimeoutSec;
        private readonly double m_maximumTimoutSec;
        private readonly double m_multiplier;
        private readonly int m_increasingThreshold;

        private int m_multiplierStep;
        private int m_callsCount;
        private TimeSpan m_value;

        public PollingTimeout()
            : this(TimeSpan.FromSeconds(INITIAL_TIMEOUT_SEC), TIMEOUT_MULTIPLIER, INCREASING_THRESHOLD, TimeSpan.FromSeconds(MAX_TIMEOUT_SEC))
        {
        }

        public PollingTimeout(
            TimeSpan initialTimeout,
            double multiplier,
            int increasingThreshold,
            TimeSpan maximumTimout)
        {
            m_initialTimeoutSec = initialTimeout.Seconds;
            m_multiplier = multiplier;
            m_increasingThreshold = increasingThreshold;
            m_maximumTimoutSec = maximumTimout.Seconds;

            Reset();
        }

        public Task WaitAsync(CancellationToken token)
        {
            // Protect against TaskCanceledException.
            return Task.WhenAny(Task.Delay(m_value, token));
        }

        public void Increase()
        {
            m_callsCount = m_callsCount + 1;

            if (m_callsCount % m_increasingThreshold == 0)
            {
                m_value = CalculateTimeoutValue(
                    m_initialTimeoutSec,
                    m_multiplier,
                    m_multiplierStep,
                    m_maximumTimoutSec);

                m_multiplierStep++;
            }
        }

        public void Reset()
        {
            m_callsCount = 0;
            m_multiplierStep = 1;
            m_value = TimeSpan.FromSeconds(m_initialTimeoutSec);
        }

        public override string ToString()
        {
            return Value.ToInvariantString();
        }

        private static TimeSpan CalculateTimeoutValue(double initialTimeout, double multiplier, int callsCount, double maximumTimout)
        {
            return TimeSpan.FromSeconds(
                Math.Min(initialTimeout * multiplier * callsCount ,
                maximumTimout));
        }

        public TimeSpan Value
        {
            get { return m_value; }
        }
    }
}
