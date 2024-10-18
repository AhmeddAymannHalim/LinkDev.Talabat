using AutoMapper;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services.Auth;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Services.Auth
{
    internal class AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager) : IAuthService
    {
        public async Task<UserDto> LoginAsync(LoginDto model)
        {           

         
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is null)
                throw new BadRequestException("Invalid Login");

            var result = await signInManager.CheckPasswordSignInAsync(user,model.Password,true);

            if (!result.Succeeded) throw new BadRequestException("Invalid Login");

            var response = new UserDto()
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = model.Email,
                Token = "This Will be JWT Token"
                
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
                Token = "This Will Be Jwt Token"
            };

            return response;
        }

    }
}
