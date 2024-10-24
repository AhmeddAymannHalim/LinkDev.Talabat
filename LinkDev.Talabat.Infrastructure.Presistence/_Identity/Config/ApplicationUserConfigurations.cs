using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkDev.Talabat.Infrastructure.Presistence._Identity.Config
{
    [DbContext(typeof(IdentityDbContext))]
    internal class ApplicationUserConfigurations : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {

            builder.Property(U => U.DisplayName)
                   .HasColumnType("varchar")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.HasOne(U => U.Address)
                   .WithOne(A => A.User)
                   .HasForeignKey<Address>(A => A.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
        //public DbSet<Address> Addresses { get; set; }


    }
}
 