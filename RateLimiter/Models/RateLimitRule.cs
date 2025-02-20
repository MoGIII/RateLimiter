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
        public string Requester { get;} 
        public List<(int Limit, TimeSpan duration)> RateLimits { get; }
        /// <summary>
        /// Creates an instance of a RateLimitRule
        /// </summary>
        /// <param name="user">The requester</param>
        /// <param name="rateLimits">List of limit amount and duration pairs</param>
        public RateLimitRule(string user, List<(int, TimeSpan)> rateLimits)
        {
            Requester = user;
            RateLimits = rateLimits;
        }
    }
}
