using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkDev.Talabat.Infrastructure.Presistence._Identity.Config
{
    internal class ApplicationUserConfigurations : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {

            builder.Property(U => U.DisplayName).IsRequired()
                                                .HasMaxLength(100)
                                                .HasColumnType("varchar");

            builder.HasOne(U => U.Address)
                   .WithOne(A => A.User)
                   .HasForeignKey<Address>(A => A.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
