using LinkDev.Talabat.Core.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinkDev.Talabat.Core.Domain.Specifications
{
    public class BaseSpecifications<TEntity, Tkey> : ISpecfifcations<TEntity, Tkey>
        where TEntity : BaseAuditableEntity<Tkey> 
        where Tkey : IEquatable<Tkey>
    {

        public Expression<Func<TEntity, bool>>? Criteria { get; set; } = null!;

        public List<Expression<Func<TEntity, object>>> Includes { get; set; } =  new();

        public Expression<Func<TEntity, object>>? OrderBy { get; set; } = null;

        public Expression<Func<TEntity, object>>? OrderByDesc { get; set; } = null;


        protected BaseSpecifications()
        {
            
        }

        protected BaseSpecifications(Expression<Func<TEntity, bool>> criateriaExpression)
        {
            Criteria = criateriaExpression;
        }

        protected BaseSpecifications(Tkey? id)
        {
            Criteria = E => E.Id.Equals(id) ;
            
        }

        private protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExpression)
        {
            OrderBy = orderByExpression;

        }
        private protected void AddOrderByDesc(Expression<Func<TEntity, object>> orderByExpressionDesc)
        {
            OrderByDesc = orderByExpressionDesc;

        }
        private protected virtual void AddIncludes()
        {
           
        }


    }
}
