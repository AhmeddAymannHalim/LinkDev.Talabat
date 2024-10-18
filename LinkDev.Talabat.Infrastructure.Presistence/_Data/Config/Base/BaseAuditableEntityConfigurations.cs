using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Infrastructure.Presistence.Data.Config.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Infrastructure.Presistence._Data.Config.Base
{
    internal class BaseAuditableEntityConfigurations<TEntity,Tkey> : BaseEntityConfigurations<TEntity,Tkey> where TEntity :BaseAuditableEntity<Tkey> where Tkey:
        IEquatable<Tkey>
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.Property(B => B.Id)
                    .ValueGeneratedOnAdd();

            builder.Property(E => E.CreatedBy)
                    .IsRequired();

            builder.Property(E => E.CreatedOn)
                  .IsRequired();

            builder.Property(E => E.LastModifiedBy) 
                  .IsRequired();

            builder.Property(E => E.LastModifiedOn)
                  .IsRequired();

        }
    }
}
