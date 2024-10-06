using LinkDev.Talabat.Core.Domain.Contracts;

namespace LinkDev.Talabat.APIs.extensions
{
    public static class IntializerExtensions
    {
        public static async Task<WebApplication> IntializeStoreContextAsync(this WebApplication app) 
        
        {
            using var scope = app.Services.CreateAsyncScope();

            var services = scope.ServiceProvider;

            var storeContextInitializer = services.GetRequiredService<IStoreContextInitializer>();//Ask Runtime Environment To Take Object From "StoreContext" Service Explicitly


            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                await storeContextInitializer.InitializeAsync();
                await storeContextInitializer.SeedAsync();

            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();

                logger.LogError(ex, "An error throw Applying Migration");
            }
            return app;

        }
    }
}
