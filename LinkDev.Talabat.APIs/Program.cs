
using LinkDev.Talabat.APIs.Controllers.Errors;
using LinkDev.Talabat.APIs.extensions;
using LinkDev.Talabat.APIs.Middlewares;
using LinkDev.Talabat.APIs.Services;
using LinkDev.Talabat.Core.Application;
using LinkDev.Talabat.Core.Application.Abstraction;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure;
using LinkDev.Talabat.Infrastructure.Presistence;
using LinkDev.Talabat.Infrastructure.Presistence._Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
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

            #region Swagger
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            webApplicationbuilder.Services.AddEndpointsApiExplorer();
            webApplicationbuilder.Services.AddSwaggerGen(); 
            #endregion


            
            webApplicationbuilder.Services.AddHttpContextAccessor();
            webApplicationbuilder.Services.AddScoped(typeof(ILoggedInUserService), typeof(LoggedInUserService));


            // LinkDev.Talabat.Core.Application
            webApplicationbuilder.Services.AddApplicationServices();

            // LinkDev.Talabat.Infrastructure.Presistence
            webApplicationbuilder.Services.AddPersistenceServices(webApplicationbuilder.Configuration);

            // LinkDev.Talabat.Infrastructure
            webApplicationbuilder.Services.AddInfrastructureServices(webApplicationbuilder.Configuration);

            // LinkDev.Talabat.Infrastructure.Presistence._Identity
            webApplicationbuilder.Services.AddIdentity<ApplicationUser, IdentityRole>(identityOptions =>
            {
                #region Confirmations On Account
                identityOptions.SignIn.RequireConfirmedAccount = true;
                identityOptions.SignIn.RequireConfirmedEmail = true;
                identityOptions.SignIn.RequireConfirmedPhoneNumber = true;
                #endregion

                #region Validation of password
                //We Made RegularExpression On The Column Password No Need For Validation here .. it will Not Go To The EndPoint and Make An Exception

                // identityOptions.Password.RequireNonAlphanumeric = true;
                // identityOptions.Password.RequiredUniqueChars = 2;
                // identityOptions.Password.RequiredLength = 6;
                // identityOptions.Password.RequireDigit = true;
                // identityOptions.Password.RequireUppercase = true;
                // identityOptions.Password.RequireLowercase = true;

                #endregion

                #region Validation Of User
                identityOptions.User.RequireUniqueEmail = true;
                //identityOptions.User.AllowedUserNameCharacters = ""; 
                #endregion

                #region LockOut Validation
                identityOptions.Lockout.AllowedForNewUsers = true;
                identityOptions.Lockout.MaxFailedAccessAttempts = 5;
                identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(12); 
                #endregion


                //identityOptions.Stores.


                //identityOptions.Tokens.

                //identityOptions.ClaimsIdentity.






            })
                .AddEntityFrameworkStores<StoreIdentityDbContext>();
            #endregion

            var app = webApplicationbuilder.Build();

            #region Database Initialization 


            // Configure the HTTP request pipeline.
             await app.IntializeDbAsync();



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
