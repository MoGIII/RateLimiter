using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiter.Models
{
    /// <summary>
    /// Class RateLimitRule represents the amount of max 
    /// requests allowed (Limit) in a period of time (Duration) 
    /// by a requester (User)
    /// </summary>
    public class RateLimitRule
    {
        public TimeSpan WindowSize { get; set; } 
        public int MaxRequests { get; set; } // 
        /// <summary>
        /// Creates an instance of a RateLimitRule
        /// </summary>
        /// <param name="windowSize">Moving window size</param>
        /// <param name="maxRequests">Max requests allowed in the window</param>
        public RateLimitRule(TimeSpan windowSize, int maxRequests)
        {
            WindowSize = windowSize;
            MaxRequests = maxRequests;
        }
    }
}
