﻿using LinkDev.Talabat.Core.Domain.Entities.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Contracts.Persistence
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IGenericRepository<TEntity, Tkey> GetRepository<TEntity, Tkey>()
            where TEntity : BaseAuditableEntity<Tkey>
            where Tkey : IEquatable<Tkey>;

        Task<int> CompleteAsync();

    }
}
