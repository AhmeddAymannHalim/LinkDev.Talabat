
using LinkDev.Talabat.APIs.Controllers.Errors;
using LinkDev.Talabat.APIs.extensions;
using LinkDev.Talabat.APIs.Middlewares;
using LinkDev.Talabat.APIs.Services;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Abstraction;
using LinkDev.Talabat.Infrastructure;
using LinkDev.Talabat.Infrastructure.Presistence;
using Microsoft.AspNetCore.Mvc;
using static LinkDev.Talabat.APIs.Controllers.Errors.ApiValidationErrorResponse;
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
                .ConfigureApiBehaviorOptions(option =>
                {
                    option.SuppressModelStateInvalidFilter = false;
                    option.InvalidModelStateResponseFactory = (actionContext) =>
                    {
                        var errors = actionContext.ModelState.Where(P => P.Value!.Errors.Count > 0)
                                       .Select(P => new ValidationError()
                                       {
                                           Fields = P.Key,
                                           Errors = P.Value!.Errors.Select(E => E.ErrorMessage)
                                       });
                        return new BadRequestObjectResult(new ApiValidationErrorResponse()
                        {
                            Errors = errors

                        });


                    };
                })
                .AddApplicationPart(typeof(Controllers.AssemblyInformation).Assembly);//Register Required Services By Asp.NetCore WebAPi to Dependancy Injection

            webApplicationbuilder.Services.AddInfrastructureServices(webApplicationbuilder.Configuration);

            #region ConfigureApiBehavior - options
            //webApplicationbuilder.Services.Configure<ApiBehaviorOptions>(option =>
            //{
            //    option.SuppressModelStateInvalidFilter = false;
            //    option.InvalidModelStateResponseFactory = (actionContext) =>
            //    {
            //        var errors = actionContext.ModelState.Where(P => P.Value!.Errors.Count > 0)
            //                       .SelectMany(E => E.Value!.Errors)
            //                       .Select(Er => Er.ErrorMessage);
            //        return new BadRequestObjectResult(new ApiValidationErrorResponse()
            //        {
            //            Errors = errors

            //        });


            //    };
            //}); 
            #endregion


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
                //app.UseDeveloperExceptionPage(); .Net 5 
            }

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseStatusCodePagesWithReExecute("/Errors/{0}");

            app.UseStaticFiles();           

            app.MapControllers();
            #endregion

            app.Run();
        }
    }
}
