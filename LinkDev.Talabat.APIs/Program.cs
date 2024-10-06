
using LinkDev.Talabat.Infrastructure.Presistence;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LinkDev.Talabat.APIs
{
    public class Program
    {
        

        public static async Task Main(string[] args)
        {
            var webApplicationbuilder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            #region Configure Services

            webApplicationbuilder.Services.AddControllers();//Register Required Services By Asp.NetCore WebAPi to Dependancy Injection  
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            webApplicationbuilder.Services.AddEndpointsApiExplorer();
            webApplicationbuilder.Services.AddSwaggerGen();

            webApplicationbuilder.Services.AddPersistenceServices(webApplicationbuilder.Configuration);

            #endregion

            #region Update-Database and Data Seeding
            var app = webApplicationbuilder.Build();

            // Configure the HTTP request pipeline.
           

            using var scope = app.Services.CreateAsyncScope();

            var services = scope.ServiceProvider;

            var dbContext = services.GetRequiredService<StoreContext>();//Ask Runtime Environment To Take Object From "StoreContext" Service Explicitly


            var loggerFactory = services.GetRequiredService<ILoggerFactory>();

            try
            {
                var pendingMigration = dbContext.Database.GetPendingMigrations();

                if (pendingMigration.Any())
                    await dbContext.Database.MigrateAsync(); // Update-Database

               await StoreContextSeed.SeedAsync(dbContext);

            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();

                logger.LogError(ex, "An error throw Applying Migration");
            }

            #endregion

            #region Configure Kestrel Middlewares
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.UseStaticFiles();

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
