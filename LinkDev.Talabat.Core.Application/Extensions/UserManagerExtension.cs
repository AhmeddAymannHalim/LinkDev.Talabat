﻿using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Application.Extentions
{
    internal static class UserManagerExtension
    {
        public static async Task<ApplicationUser?> FindUserWithAddress(this UserManager<ApplicationUser> userManager,ClaimsPrincipal claimsPrincipal)
        {
            var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);

            var user = await userManager.Users.Where(user => user.Email == email).Include(user => user.Address).FirstOrDefaultAsync();

            return user;
        }
    }
}
