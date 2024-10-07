using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Infrastructure.Presistence.Repositories
{
    internal class GenericRepository<TEntity, Tkey>(StoreContext _dbContext) : IGenericRepository<TEntity, Tkey>
        where TEntity : BaseAuditableEntity<Tkey> where Tkey : IEquatable<Tkey>
    {
        
        public async Task<IEnumerable<TEntity>> GetAllAsync(bool withTracking = false)

            => withTracking ? await _dbContext.Set<TEntity>().ToListAsync() : await _dbContext.Set<TEntity>().AsNoTracking().ToListAsync();

        
        public async Task<TEntity?> GetAsync(Tkey id)
         =>  await _dbContext.Set<TEntity>().FindAsync(id);



        public async Task AddAsync(TEntity entity)
           => await _dbContext.Set<TEntity>().AddAsync(entity);


        public void Update(TEntity entity)
             => _dbContext.Set<TEntity>().Update(entity);


        public void Delete(TEntity entity)
            => _dbContext.Set<TEntity>().Remove(entity);
            


      
    }
}
