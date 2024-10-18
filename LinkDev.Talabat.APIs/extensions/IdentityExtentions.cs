using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Application.Services.Auth;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure.Presistence._Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace LinkDev.Talabat.APIs.extensions
{
    public static class IdentityExtentions
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services , IConfiguration configuration)
        {


            services.Configure<JwtSettings>(configuration.GetSection("JWTSettings"));

            services.AddScoped(typeof(IAuthService), typeof(AuthService));

            services.AddScoped(typeof(Func<IAuthService>), (serviceProvider) =>
            {
                var usermanager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var signinManager = serviceProvider.GetRequiredService<SignInManager<ApplicationUser>>();
                var jwtSettings = serviceProvider.GetRequiredService<IOptions<JwtSettings>>();

                return () => new AuthService(usermanager, signinManager, jwtSettings);

            });
            services.AddIdentity<ApplicationUser, IdentityRole>(identityOptions =>
            {
                #region Confirmations On Account
                // identityOptions.SignIn.RequireConfirmedAccount = true;
                // identityOptions.SignIn.RequireConfirmedEmail = true;
                // identityOptions.SignIn.RequireConfirmedPhoneNumber = true;
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
            return services;
        }
    }
}
