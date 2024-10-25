using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Application.Exceptions;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LinkDev.Talabat.Core.Application.Services.Auth
{
    public class AuthService(
        UserManager<ApplicationUser>     userManager,
        SignInManager<ApplicationUser>   signInManager,
        IOptions<JwtSettings>            jwtSettings
        ) : IAuthService
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<UserDto> LoginAsync(LoginDto model)
        {                    
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is null)
                throw new UnAuthorizedException("Invalid Login");

            var result = await signInManager.CheckPasswordSignInAsync(user,model.Password,lockoutOnFailure: true);

            if (result.IsNotAllowed) throw new UnAuthorizedException("Account not Confirmed yet.");

            if(result.IsLockedOut) throw new UnAuthorizedException("Account is locked.");

            //if (result.RequiresTwoFactor) throw new UnAuthorizedException("Required Two-Factor Authentication");


            if(!result.Succeeded)
                throw new UnAuthorizedException("Invalid Login");

            var response = new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = model.Email,
                Id = user.Id,
                Token = await GenerateTokenAsync(user)
                
            };

            return response;
        }

        public async Task<UserDto> RegisterAsync(RegisterDto model)
        {
            var user = new ApplicationUser()
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.UserName,
                PhoneNumber = model.Phone,

            };
            var result = await userManager.CreateAsync(user,model.Password);
            if (!result.Succeeded) throw new Exceptions.ValidationException() { Errors = result.Errors.Select(E => E.Description) };

            var response = new UserDto()
            {
                DisplayName = user.DisplayName,
                Email = user.Email,
                Id = user.Id,
                Token = await GenerateTokenAsync(user)
            };

            return response;
        }

        private async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            var userClaims = await userManager.GetClaimsAsync(user);
            var roleAsClaims = new List<Claim>();

            var roles = await userManager.GetRolesAsync(user);
            foreach (var role in roles)
                roleAsClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));

            //Private Claims
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.PrimarySid,user.Id),
                new Claim(ClaimTypes.Email,user.Email!),
                new Claim(ClaimTypes.Name,user.DisplayName),

            }.Union(userClaims)
             .Union(roleAsClaims).ToList();
            //var userClaims = userManager.GetClaimsAsync(user);
          
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var signingCredentials = new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256);

            var TokenObject = new JwtSecurityToken(

                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                claims: claims,
                signingCredentials: signingCredentials

                );

            return new JwtSecurityTokenHandler().WriteToken(TokenObject);
        }



    }
}
