using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using SalonAPI.Models;
using System.Threading.Tasks;

namespace SalonAPI.Utils
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CustomMiddleware
    {
        private readonly RequestDelegate _next;
        

        public CustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext, DataContext context)
        {

            context.LogEntries.Add(new LogEntry()
            {
                Content = $"http request with following path: {httpContext.Request.Path}",
                LogCategory = LogCategory.info
            }
            );



            return _next(httpContext);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CustomMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CustomMiddleware>();
        }
    }
}
