using LinkDev.Talabat.Core.Domain.Contracts;
using LinkDev.Talabat.Core.Domain.Entities.Products;
using LinkDev.Talabat.Infrastructure.Presistence.Data;
using LinkDev.Talabat.Infrastructure.Presistence.Repositories;

namespace LinkDev.Talabat.Infrastructure.Presistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _dbContext;
        private readonly Lazy<IGenericRepository<Product, int>> _productRepository;      //Product
        private readonly Lazy<IGenericRepository<ProductBrand, int>> _brandRepository;     //Brand
        private readonly Lazy<IGenericRepository<ProductCategory, int>> _categoryRepository;  //Category
        public UnitOfWork(StoreContext dbContext)
        {
            _dbContext = dbContext;
            //Product
            _productRepository = new Lazy<IGenericRepository<Product, int>>(() => new GenericRepository<Product, int>(_dbContext));
            //Brand
            _brandRepository = new Lazy<IGenericRepository<ProductBrand, int>>(() => new GenericRepository<ProductBrand,int>(_dbContext));
            //Category
            _categoryRepository = new Lazy<IGenericRepository<ProductCategory, int>>(() => new GenericRepository<ProductCategory, int>(_dbContext));

        }


        public IGenericRepository<Product, int> ProductRepository => _productRepository.Value;


        public IGenericRepository<ProductBrand, int> BrandsRepository => _brandRepository.Value;

        public IGenericRepository<ProductCategory, int> CategoriesRepository => _categoryRepository.Value;


        public Task<int> CompleteAsync()
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
