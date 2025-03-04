using RateLimiter.Models;
using System.Collections.Concurrent;

namespace RateLimiter
{

    /// <summary>
    /// Class RateLimiter implements a rate limiter using the 
    /// sliding window approach.
    /// This approach won't block the user completly until the fixed time has passed
    /// and will allow smoother spread of the requests over time. 
    /// </summary>
    
    public class RateLimiter
    {
        //A function forwarded from the user to create a list of limit rules for each requester
        private readonly ConcurrentDictionary<string, List<DateTime>> _requestLogs = new();
        private readonly List<RateLimitRule> _rateLimits;
        private readonly Func<string> _getUserId;

        /// <summary>
        /// Creates an Instance of RateLimiter 
        /// </summary>
        
        public RateLimiter(List<RateLimitRule> rateLimits, Func<string> getUserId)
        {
            _rateLimits = rateLimits;
            _getUserId = getUserId;
        }

        /// <summary>
        /// Method to check if the rate limit was reached in any of the request queues.
        /// </summary>
        /// <returns>true if limit was reached, else false</returns>
        public bool IsLimitReached()
        {
            //Extract rule by requester
            string userId = _getUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            DateTime now = DateTime.UtcNow;

            if (!_requestLogs.ContainsKey(userId))
            {
                _requestLogs[userId] = new List<DateTime>();
                
            }
            var timestamps = _requestLogs[userId];
            // Remove old requests outside of all windows

            foreach (var item in _rateLimits)
            {
                timestamps.RemoveAll(stamp => stamp < now - item.WindowSize);
            }

            //Check for every rule that the limit was passed
            foreach (var rule in _rateLimits)
            {
                int requestCount = timestamps.Count;
                if (requestCount >= rule.MaxRequests)
                {
                    return false;
                }
            }
            timestamps.Add(now);
            return true;
        }
    }
}
