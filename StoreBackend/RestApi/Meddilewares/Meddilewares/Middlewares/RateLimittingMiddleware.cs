namespace Meddilewares.Middlewares
{
    public class RateLimittingMiddleware
    {
        private readonly RequestDelegate next;
        private static int _counter = 0;
        private static DateTime lastRequestDate = DateTime.Now;

        public RateLimittingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            _counter++;

            if(DateTime.Now.Subtract(lastRequestDate).Seconds > 10)
            {
                _counter = 1;
                lastRequestDate = DateTime.Now;
                await next(context);
            }
            else
            {
                if(_counter > 5)
                {
                    lastRequestDate = DateTime.Now;
                    await context.Response.WriteAsync("Rate limit exceeded");
                }
                else
                {
                    lastRequestDate = DateTime.Now;
                    await next(context);
                }
            }

        }
    }
}
