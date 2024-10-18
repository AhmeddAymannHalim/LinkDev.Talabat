using LinkDev.Talabat.APIs.Controllers.Controllers.Base;
using LinkDev.Talabat.Core.Application.Abstraction.Models.Auth;
using LinkDev.Talabat.Core.Application.Abstraction.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.APIs.Controllers.Controllers.Account
{
    public class AccountController(IServiceManager serviceManager) : ApiControllerBase
    {

        [HttpPost("login")] // Post => /api/account/login
         
        public async Task<ActionResult<UserDto>> Login(LoginDto model)
        {
            var response = await serviceManager.AuthService.LoginAsync(model);

            return Ok(response);
        }


        [HttpPost("register")] // Post => /api/account/register

        public async Task<ActionResult<RegisterDto>> Register(RegisterDto model)
        {
            var respose = await serviceManager.AuthService.RegisterAsync(model);
            return Ok(respose);
        }



    }
}
