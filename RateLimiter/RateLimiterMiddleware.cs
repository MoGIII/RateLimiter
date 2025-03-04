using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RateLimiter
{
    public class RateLimiterMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RateLimiter _rateLimiter;

        public RateLimiterMiddleware(RequestDelegate next, RateLimiter rateLimiter)
        {
            _next = next;
            _rateLimiter = rateLimiter;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            if (!_rateLimiter.IsLimitReached())
            {
                httpContext.Response.StatusCode = 429;
                await httpContext.Response.WriteAsync("Rate limit was exceeded. Please try again later.");
                return;
            }

            await _next(httpContext);
        }
    }
}
