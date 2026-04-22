using System.Diagnostics;

namespace Meddilewares.Meddilewares
{
    public class ProfileingMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ProfileingMiddleware> logger;

        public ProfileingMiddleware(RequestDelegate next,ILogger<ProfileingMiddleware> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            await next(context);
            stopwatch.Stop();
            logger.LogInformation($"Request {context.Request.Path} Request took {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
