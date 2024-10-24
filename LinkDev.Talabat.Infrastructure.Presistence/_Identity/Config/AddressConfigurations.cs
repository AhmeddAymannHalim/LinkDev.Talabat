using LinkDev.Talabat.Core.Domain.Entities._Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkDev.Talabat.Infrastructure.Presistence.Identity.Config
{
    [DbContext(typeof(IdentityDbContext))]
    internal class AddressConfigurations : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {

            builder.Property(nameof(Address.Id)).ValueGeneratedOnAdd();
            builder.Property(nameof(Address.FirstName))
                   .HasColumnType("nvarchar")
                   .HasMaxLength(50);


            builder.Property(nameof(Address.LastName))
                   .HasColumnType("nvarchar")
                   .HasMaxLength(50);   
            
            builder.Property(nameof(Address.Street))
                   .HasColumnType("varchar")
                   .HasMaxLength(50);
            
            builder.Property(nameof(Address.City))
                   .HasColumnType("varchar")
                   .HasMaxLength(50);

            builder.Property(nameof(Address.Country))
                   .HasColumnType("varchar")
                   .HasMaxLength(50);

            builder.ToTable("Addresses");
        }
    }
}
