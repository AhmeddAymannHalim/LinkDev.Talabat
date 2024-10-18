using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Application.Exceptions;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Services.Auth
{
    public class AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IOptions<JwtSettings> jwtSettings
        ) : IAuthService
    {
        private readonly JwtSettings _jwtSettings = jwtSettings.Value;

        public async Task<UserDto> LoginAsync(LoginDto model)
        {           

         
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is null)
                throw new UnAuthorizedException("Invalid Login");

            var result = await signInManager.CheckPasswordSignInAsync(user,model.Password,true);

            if (!result.Succeeded) throw new UnAuthorizedException("Invalid Login");

            var response = new UserDto()
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = model.Email,
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

            //Private Claims
            var privateClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.PrimarySid,user.Id),
                new Claim(ClaimTypes.Email,user.Email!),
                new Claim(ClaimTypes.Name,user.DisplayName),

            }.Union(await userManager.GetClaimsAsync(user)).ToList();
            //var userClaims = userManager.GetClaimsAsync(user);
          
                foreach (var role in await userManager.GetRolesAsync(user)) 
                privateClaims.Add(new Claim(ClaimTypes.Role,role.ToString()));

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));

            var TokenObject = new JwtSecurityToken(

                audience: _jwtSettings.Audience,
                issuer: _jwtSettings.Issuer,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                claims: privateClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256)

                );

            return new JwtSecurityTokenHandler().WriteToken(TokenObject);
        }



    }
}
