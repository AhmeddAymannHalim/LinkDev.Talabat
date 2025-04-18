﻿using LinkDev.Talabat.APIs.Controllers.Controllers.Base;
using LinkDev.Talabat.Core.Application.Abstraction.Models._Common;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Controllers.Account
{
    public class AccountController(IServiceManager serviceManager) : ApiControllerBase
    {

        [HttpPost("login")] // Post => /api/account/login       
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var result = await serviceManager.AuthService.LoginAsync(model);
            return Ok(result);
        }


        [HttpPost("register")] // Post => /api/account/register
        public async Task<ActionResult<RegisterDto>> Register(RegisterDto model)
        {
            var result = await serviceManager.AuthService.RegisterAsync(model);
            return Ok(result);
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var result = await serviceManager.AuthService.GetCurrentUser(User);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("address")]
        public async Task<ActionResult<AddressDto>> GetUserAddress()
        {
            var result = await serviceManager.AuthService.GetUserAddress(User);

            return Ok(result);
        }


        [Authorize]
        [HttpPut("address")]
        public async Task<ActionResult<AddressDto>> UpdateUserAddress(AddressDto addressDto)
        {
           
            var result = await serviceManager.AuthService.UpdateUserAddress(User,addressDto);

            return Ok(result);
        }

        [HttpGet("emailexists")]
        public async Task<ActionResult<bool>> CheckEmailExists(string email)
        {
            return Ok(await serviceManager.AuthService.EmailExists(email!));
        }


    }
}
