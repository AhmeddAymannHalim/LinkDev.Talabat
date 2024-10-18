using LinkDev.Talabat.Core.Domain.Contracts.Persistence.DbInitializers;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Infrastructure.Presistence._Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Infrastructure.Presistence._Identity
{
    internal sealed class StoreIdentityDbInitializer(StoreIdentityDbContext _dbContext, UserManager<ApplicationUser> userManager) : DbInitializer(_dbContext), IStoreIdentityDbInitializer
    {
        public override async Task SeedAsync()
        {
            var user = new ApplicationUser()
            {
                DisplayName = "Ahmed Nasr",
                UserName    = "ahmed.nasr",
                Email       = "ahmed.nasr@linkdev.com",
                PhoneNumber = "01021487569",
                
            };
            await userManager.CreateAsync(user,"P@ssw0rd");
        }
        
    }
}
