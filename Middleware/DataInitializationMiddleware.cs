using System;
using System.Threading.Tasks;
using TravelSaaS.Services;

namespace TravelSaaS.Middleware
{
    public class DataInitializationMiddleware
    {
		private readonly RequestDelegate _next;
		private static bool _initialized = false;
		private static readonly object _lock = new object();

		public DataInitializationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context, IServiceProvider serviceProvider)
		{
			// S'assurer que l'initialisation ne se fait qu'une seule fois
			if (!_initialized)
			{
				lock (_lock)
				{
					if (!_initialized)
					{
						using (var scope = serviceProvider.CreateScope())
						{
							var initializer = scope.ServiceProvider.GetRequiredService<DataInitializer>();
							initializer.InitializeAsync().Wait(); // Attention: en production, utilisez une approche async correcte
						}
						_initialized = true;
					}
				}
			}

			await _next(context);
		}
	}

}
}
