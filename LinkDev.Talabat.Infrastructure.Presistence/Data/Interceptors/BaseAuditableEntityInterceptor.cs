using LinkDev.Talabat.Core.Application.Abstraction;
using LinkDev.Talabat.Core.Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LinkDev.Talabat.Infrastructure.Presistence.Data.Interceptors
{
    public class BaseAuditableEntityInterceptor :SaveChangesInterceptor
    {
        private readonly ILogedInUserService _logedInUserService;

        public BaseAuditableEntityInterceptor(ILogedInUserService logedInUserService)
        {
            _logedInUserService = logedInUserService;
        }


        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateEntities(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {

            UpdateEntities(eventData.Context);
            return base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateEntities(DbContext? dbContext)
        {
            if (dbContext is null) return;

            var utcNow = DateTime.UtcNow;

            foreach (var entity in dbContext.ChangeTracker.Entries<BaseAuditableEntity<int>>())
            {
                if (entity is { State: EntityState.Added or EntityState.Modified })
                {
                    entity.Entity.CreatedBy = "";
                    entity.Entity.CreatedOn = utcNow;
                }

                entity.Entity.LastModifiedBy = "";
                entity.Entity.LastModifiedOn = utcNow;
            }

        }
    }
}
