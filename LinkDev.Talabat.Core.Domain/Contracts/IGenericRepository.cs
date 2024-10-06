namespace LinkDev.Talabat.Core.Domain.Contracts
{
    public interface IGenericRepository<TEntity,Tkey> 
        where TEntity : BaseEntity<Tkey> where Tkey : IEquatable<Tkey>
    {
        Task<IEnumerable<TEntity>> GetAllAsync(bool withTracking = false);

        Task<TEntity?> GetAsync(Tkey id);

        Task AddAsync(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

    }
}
