using LinkDev.Talabat.Core.Domain.Common;
using LinkDev.Talabat.Core.Domain.Contracts;
using Microsoft.IdentityModel.Tokens;

namespace LinkDev.Talabat.Infrastructure.Presistence.Repositories.GenericRepository
{
    internal class SpecificationsEvaluator<TEntity,Tkey> 
        where TEntity : BaseAuditableEntity<Tkey>
        where Tkey : IEquatable<Tkey>
    {

        public static IQueryable<TEntity> GetQuery(IQueryable<TEntity> inputQuery ,ISpecfifcations<TEntity,Tkey> spec)
        {

            var query = inputQuery; //_dbContext.Set<Product>()

            if (spec.Criteria is not null) // P => P.BrandId == 1 && P => P.CategoryId == 1
                query = query.Where(spec.Criteria);

            if (spec.OrderByDesc is not null)
                query = query.OrderByDescending(spec.OrderByDesc);
            else if (spec.OrderBy is not null)
                query = query.OrderBy(spec.OrderBy);


            //query = _dbContext.Set<Product>().Where(P => P.Id == 10)
            //include Expression
            //1.P => P.Brand
            //2.P => P.Category 
            //..
            query = spec.Includes.Aggregate(query, (currentQuery, include) => currentQuery.Include(include));


            return query;
        }
    }
}
