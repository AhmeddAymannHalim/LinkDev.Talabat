using LinkDev.Talabat.Core.Domain.Contracts.Persistence.DbInitializers;

namespace LinkDev.Talabat.APIs.extensions
{
    public static class IntializerExtensions
    {
        public static async Task<WebApplication> IntializeDbAsync(this WebApplication app) 
        
        {
            using var scope = app.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;

            #region StoreContextInitializer
            var storeContextInitializer = services.GetRequiredService<IStoreDbInitializer>();
            #endregion

            #region IdentityInitializer
            var storeIDentityInitializer = services.GetRequiredService<IStoreIdentityDbInitializer>();

            #endregion

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                #region StoreContextInitializer
                await storeContextInitializer.InitializeAsync();
                await storeContextInitializer.SeedAsync();
                #endregion

                #region IdentityInitializer
                await storeIDentityInitializer.InitializeAsync();
                await storeIDentityInitializer.SeedAsync();

                #endregion
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
