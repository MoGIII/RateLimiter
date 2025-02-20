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
    /// <typeparam name="TArg">Type of argument the passed method recieves</typeparam>
    public class RateLimiter<TArg>
    {
        //A function forwarded from the user to create a list of limit rules for each requester
        private readonly Func<string, RateLimitRule> _limits;

        //A static dictionary that can be accessed concurrently for storing each requester and his queues of requests
        private static readonly ConcurrentDictionary<string, List<Queue<DateTime>>> _requests = new();

        //The task we want to perform is the rate limit is not reached
        private readonly Func<TArg,Task> _func;
        
        /// <summary>
        /// Creates an Instance of RateLimiter 
        /// </summary>
        /// <param name="func">The action we want to perform</param>
        /// <param name="limits">A list of rate limits per requester</param>
        public RateLimiter(Func<TArg,Task> func, Func<string, RateLimitRule> limits)
        {
            _func = func;
            _limits = limits;
        }

        /// <summary>
        /// Method to check if the rate limit was reached in any of the request queues.
        /// </summary>
        /// <param name="id">the requester</param>
        /// <param name="waitAmount">The amount of time left for the earliest request to expiret</param>
        /// <returns>true if limit was reached, else false</returns>
        public bool IsLimitReached(string id, out TimeSpan waitAmount)
        {
            //Extract rule by requester
            var rule = _limits(id);
            if (rule == null || rule.RateLimits.Count == 0)
            {
                waitAmount = TimeSpan.Zero;
                return false;
            }

            DateTime now = DateTime.UtcNow;

            //Check if the requester exists in the requests dictionary. If not - add him, else - return the requester queues list
            var queueList = _requests.GetOrAdd(id, _ => rule.RateLimits.Select(_ => new Queue<DateTime>()).ToList());

            //Lock the queues list for thread saftey
            lock (queueList)
            {
                waitAmount = TimeSpan.Zero;

                for (int i = 0; i < rule.RateLimits.Count; i++)
                {
                    //extract limit and duration for the specific queue
                    var (limit, duration) = rule.RateLimits[i];
                    var queue = queueList[i];

                    //Remove expired timestamps
                    while (queue.Count > 0 && (now - queue.Peek()) > duration)
                    {
                        queue.Dequeue();
                    }

                    //If limit was exceeded
                    if (queue.Count >= limit)
                    {
                        waitAmount = duration - (now - queue.Peek());
                        return true;
                    }
                }

                //add the current timestamp to every queue
                foreach (var item in queueList)
                {
                    item.Enqueue(now);
                }
            }
            return false;
        }

        /// <summary>
        /// Invoke the task if the limit was not reached
        /// </summary>
        /// <param name="argument">the parameter that needed to Invoke the method</param>
        public async Task Perform(TArg argument)
        {
            foreach (var item in _requests)
            {
                //Check if limit was reached
                if (!IsLimitReached(item.Key, out TimeSpan waitAmount))
                {
                    await Task.Delay(waitAmount);
                }
                await _func(argument);
            }
        }
    }
}
