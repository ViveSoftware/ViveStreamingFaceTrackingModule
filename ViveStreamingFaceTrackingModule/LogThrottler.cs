using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViveStreamingFaceTrackingModule
{
    public static class LogThrottler
    {
        private static readonly TimeSpan Duration = TimeSpan.FromSeconds(60);
        private static readonly ConcurrentDictionary<string, DateTime> LastLoggedTimes = new ConcurrentDictionary<string, DateTime>();

        public static bool ShouldLog(string message)
        {
            // Check if the message has been logged recently
            if (LastLoggedTimes.TryGetValue(message, out var lastLoggedTime))
            {
                if ((DateTime.UtcNow - lastLoggedTime) < Duration)
                {
                    // Skip logging if within throttle duration
                    return false;
                }
            }

            // Update last logged time and log the message
            LastLoggedTimes[message] = DateTime.UtcNow;
            return true;
        }
    }
}