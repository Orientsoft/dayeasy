using DayEasy.Core.Domain.Entities;
using DayEasy.Core.Domain.Repositories;
using DayEasy.EntityFramework;

namespace DayEasy.Services
{
    public interface IDayEasyRepository<TEntity> : IDayEasyRepository<TEntity, string>
        where TEntity : DEntity<string> { }

    public interface IDayEasyRepository<TEntity, TPrimaryKey> : IRepository<TEntity, TPrimaryKey>
        where TEntity : DEntity<TPrimaryKey> { }

    public class DayEasyRepository<TEntity, TKey>
        : EfRepository<DayEasyDbContext, TEntity, TKey>, IDayEasyRepository<TEntity, TKey>
        where TEntity : DEntity<TKey>
    {
        public DayEasyRepository(IDbContextProvider<DayEasyDbContext> unitOfWork)
            : base(unitOfWork)
        {
        }
    }

    public class DayEasyRepository<TEntity>
        : DayEasyRepository<TEntity, string>, IDayEasyRepository<TEntity>
        where TEntity : DEntity<string>
    {
        public DayEasyRepository(IDbContextProvider<DayEasyDbContext> unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
