using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using LinkDev.Talabat.Infrastructure.Presistence.Repositories;
using System.Collections.Concurrent;

namespace LinkDev.Talabat.Infrastructure.Presistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _dbContext;
        private readonly ConcurrentDictionary<string, object> _repositories;


        public UnitOfWork(StoreContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new ();

        }
  
        public IGenericRepository<TEntity, Tkey> GetRepository<TEntity, Tkey>()
            where TEntity : BaseEntity<Tkey>
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
