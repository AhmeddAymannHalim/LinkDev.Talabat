using LinkDev.Talabat.Core.Domain.Contracts.Persistence.DbInitializers;
using LinkDev.Talabat.Core.Domain.Entities._Identity;
using LinkDev.Talabat.Infrastructure.Presistence._Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace LinkDev.Talabat.Infrastructure.Presistence._Identity
{
    internal sealed class StoreIdentityDbInitializer(
        StoreIdentityDbContext _dbContext,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration) : DbInitializer(_dbContext), IStoreIdentityDbInitializer
    {
        public override async Task SeedAsync()
        {
            var password = configuration["Seed:DemoUserPassword"];
            if (string.IsNullOrEmpty(password) || userManager.Users.Any())
                return;

            var user = new ApplicationUser()
            {
                DisplayName = "Demo User",
                UserName = "demo.user",
                Email = "demo@talabat.local",
                PhoneNumber = "01000000000",
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    "Could not seed the demo user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}
