using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using LinkDev.Talabat.Infrastructure.Presistence.Data.Config.Base;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LinkDev.Talabat.Infrastructure.Presistence._Data.Config.Base
{
    [DbContext(typeof(StoreDbContext))]
    internal class BaseAuditableEntityConfigurations<TEntity,Tkey> : BaseEntityConfigurations<TEntity,Tkey> where TEntity :BaseAuditableEntity<Tkey> where Tkey:
        IEquatable<Tkey>
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.Property(E => E.CreatedBy);
                    //.IsRequired();

            builder.Property(E => E.CreatedOn);
                  //.IsRequired();
            //.HasDefaultValueSql("GETUTCDATE()")

            builder.Property(E => E.LastModifiedBy);
            //.IsRequired();

            builder.Property(E => E.LastModifiedOn);
                  //.IsRequired();

        }
    }
}
