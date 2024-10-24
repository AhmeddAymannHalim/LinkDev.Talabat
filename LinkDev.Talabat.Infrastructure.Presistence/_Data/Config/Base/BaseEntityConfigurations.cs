





using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkDev.Talabat.Infrastructure.Presistence.Data.Config.Base
{

    [DbContext(typeof(StoreDbContext))]
    internal class BaseEntityConfigurations<TEntity, Tkey> : IEntityTypeConfiguration<TEntity>
        where TEntity : BaseAuditableEntity<Tkey> where Tkey : IEquatable<Tkey>
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
          

            builder.Property(E => E.Id)
                  .ValueGeneratedOnAdd();

            //builder.Property(E => E.CreatedOn)
            //       .IsRequired();
            ////.HasDefaultValueSql("GETUTCDATE()");

            //builder.Property(E => E.CreatedBy)
            //      .IsRequired();

            //builder.Property(E => E.LastModifiedBy)
            //      .IsRequired();

            //builder.Property(E => E.LastModifiedOn)
            //    .IsRequired();
            ////.HasDefaultValueSql("GETUTCDATE()");

        }
    }
}
