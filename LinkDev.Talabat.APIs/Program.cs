
using LinkDev.Talabat.APIs.extensions;
using LinkDev.Talabat.APIs.Services;
using LinkDev.Talabat.Core.Application.Abstraction;
using LinkDev.Talabat.Infrastructure.Presistence;
using LinkDev.Talabat.Core.Application;
namespace LinkDev.Talabat.APIs
 
{
    public class Program
    {
        

        public static async Task Main(string[] args)
        {
            var webApplicationbuilder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            #region Configure Services

            webApplicationbuilder.Services
                .AddControllers()
                .AddApplicationPart(typeof(Controllers.AssemblyInformation).Assembly);//Register Required Services By Asp.NetCore WebAPi to Dependancy Injection  
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            webApplicationbuilder.Services.AddEndpointsApiExplorer();
            webApplicationbuilder.Services.AddSwaggerGen();
            //webApplicationbuilder.Services.AddScoped(typeof(IHttpContextAccessor),typeof(HttpContextAccessor));
            webApplicationbuilder.Services.AddHttpContextAccessor();
            webApplicationbuilder.Services.AddScoped(typeof(ILoggedInUserService), typeof(LoggedInUserService));
            webApplicationbuilder.Services.AddPersistenceServices(webApplicationbuilder.Configuration);


            webApplicationbuilder.Services.AddApplicationServices();
            #endregion

            var app = webApplicationbuilder.Build();

            #region Database Initialization 


            // Configure the HTTP request pipeline.
             await app.IntializeStoreContextAsync();



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
