using DayEasy.Core.Domain.Entities;
using DayEasy.EntityFramework;

namespace DayEasy.Services
{
    public interface IVersion3Repository<TEntity> : IDayEasyRepository<TEntity, string>
        where TEntity : DEntity<string> { }

    public interface IVersion3Repository<TEntity, TPrimaryKey> : IDayEasyRepository<TEntity, TPrimaryKey>
        where TEntity : DEntity<TPrimaryKey> { }

    public class Version3Repository<TEntity, TKey>
        : EfRepository<Version3DbContext,TEntity, TKey>, IVersion3Repository<TEntity, TKey>
        where TEntity : DEntity<TKey>
    {
        public Version3Repository(IDbContextProvider<Version3DbContext> unitOfWork)
            : base(unitOfWork)
        {
        }
    }

    public class Version3Repository<TEntity>
        : Version3Repository<TEntity, string>, IVersion3Repository<TEntity>
        where TEntity : DEntity<string>
    {
        public Version3Repository(IDbContextProvider<Version3DbContext> unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
