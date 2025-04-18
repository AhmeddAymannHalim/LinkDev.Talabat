﻿using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using LinkDev.Talabat.Infrastructure.Presistence.GenericRepository;
using System.Collections.Concurrent;

namespace LinkDev.Talabat.Infrastructure.Presistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreDbContext _dbContext;
        private readonly ConcurrentDictionary<string, object> _repositories;


        public UnitOfWork(StoreDbContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new ();

        }
  
        public IGenericRepository<TEntity, Tkey> GetRepository<TEntity, Tkey>()
            where TEntity : BaseAuditableEntity<Tkey>
            where Tkey : IEquatable<Tkey>
        {
            //return new GenericRepository<TEntity, Tkey>(_dbContext);

            ///var typeName = typeof(TEntity).Name;
            ///
            ///if (_repositories.ContainsKey(typeName)) return (IGenericRepository<TEntity, Tkey>)_repositories[typeName];
            ///
            ///var repository = new GenericRepository<TEntity, Tkey>(_dbContext);
            ///
            ///_repositories.Add(typeName, repository);
            ///
            ///return repository;
            ///


            return (IGenericRepository<TEntity, Tkey>)_repositories.GetOrAdd(typeof(TEntity).Name, new GenericRepository<TEntity, Tkey>(_dbContext));
        }

        public async Task<int> CompleteAsync() => await _dbContext.SaveChangesAsync();

        public async ValueTask DisposeAsync() => await _dbContext.DisposeAsync();
    }
}
