using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Application.Services.Auth;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure.Presistence._Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace LinkDev.Talabat.APIs.extensions
{
    public static class IdentityExtentions
    {
        public static IServiceCollection AddIdentityServices
            (
            this IServiceCollection services , 
            IConfiguration configuration
            )
        {


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
                identityOptions.Lockout.MaxFailedAccessAttempts = 10;
                identityOptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                #endregion


                //identityOptions.Stores.


                //identityOptions.Tokens.

                //identityOptions.ClaimsIdentity.






            })
               .AddEntityFrameworkStores<StoreIdentityDbContext>();

            services.Configure<JwtSettings>(configuration.GetSection("JWTSettings"));

            services.AddScoped(typeof(IAuthService), typeof(AuthService));
            services.AddScoped(typeof(Func<IAuthService>), (serviceProvider) =>
            {
                  return () => serviceProvider.GetRequiredService<IAuthService>();

            });

            services.AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                authenticationOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]!)),
                    ClockSkew = TimeSpan.Zero
                    
                };
            });
            return services;
        }
    }
}
