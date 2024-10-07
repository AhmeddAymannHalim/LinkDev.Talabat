using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts;
using LinkDev.Talabat.Core.Domain.Contracts.Persistence;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Core.Domain.Specifications.Product_Specs;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Infrastructure.Presistence.Repositories.GenericRepository
{
    internal class GenericRepository<TEntity, Tkey>(StoreContext DbContext) : IGenericRepository<TEntity, Tkey>
        where TEntity : BaseAuditableEntity<Tkey> where Tkey : IEquatable<Tkey>
    {

        public async Task<IEnumerable<TEntity>> GetAllAsync(bool withTracking = false)
        {
            

            if (typeof(TEntity).Equals(typeof(Product)))
            {
                return (IEnumerable<TEntity>)(withTracking ? await DbContext.Set<Product>().Include(P => P.Brand).Include(P => P.Category).ToListAsync() :
                         await DbContext.Set<Product>().Include(P => P.Brand).Include(P => P.Category).AsNoTracking().ToListAsync());
            }

            return withTracking ? await DbContext.Set<TEntity>().ToListAsync() :
            await DbContext.Set<TEntity>().AsNoTracking().ToListAsync();

        }


        public async Task<TEntity?> GetAsync(Tkey id)
        {
            //var specs = new ProductWithBrandAndCategorySpecifications(id);
            if (typeof(TEntity).Equals(typeof(Product)))
            {
                return await DbContext.Set<Product>().Where(P => P.Id.Equals(id)).Include(P => P.Brand).Include(P => P.Category).FirstOrDefaultAsync() as TEntity;

            }

            return await DbContext.Set<TEntity>().FindAsync(id);
        }



     

        public async Task<IEnumerable<TEntity>> GetAllWithSpecAsync(ISpecfifcations<TEntity, Tkey> spec, bool withTracking = false)
        {
            return await ApplySpecification( spec).ToListAsync();
        }

        public async Task<TEntity?> GetWithSpecAsync(ISpecfifcations<TEntity, Tkey> spec)
        {
            return await ApplySpecification(spec).FirstOrDefaultAsync();
        }


        public async Task AddAsync(TEntity entity)
        => await DbContext.Set<TEntity>().AddAsync(entity);

        public void Update(TEntity entity)
             => DbContext.Set<TEntity>().Update(entity);

        public void Delete(TEntity entity)
            => DbContext.Set<TEntity>().Remove(entity);

        #region Helpers
        private  IQueryable<TEntity> ApplySpecification( ISpecfifcations<TEntity, Tkey> spec)
        {
            return SpecificationsEvaluator<TEntity, Tkey>.GetQuery(DbContext.Set<TEntity>(), spec);
        }

        #endregion
    }
}
