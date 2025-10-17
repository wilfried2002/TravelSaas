using Microsoft.AspNetCore.Builder;

namespace TravelSaaS.Middleware
{
    public static class DataInitializationMiddlewareExtensions
    {
        public static IApplicationBuilder UseDataInitialization(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<DataInitializationMiddleware>();
        }
    }
}
