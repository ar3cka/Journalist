using System;
using Journalist.Collections;

namespace Journalist.EventStore.Notifications.Types
{
    internal static class NotificationPropertyParser
    {
        private static readonly string[] s_separators = ": ".YieldArray();

        public static void Parse(string keyValuePair, out string key, out string value)
        {
            var pair = keyValuePair.Split(s_separators, StringSplitOptions.RemoveEmptyEntries);

            key = pair[0];
            value = pair[1];
        }
    }
}
