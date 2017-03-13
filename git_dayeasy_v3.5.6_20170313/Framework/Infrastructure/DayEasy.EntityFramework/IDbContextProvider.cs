
using DayEasy.Core.Domain.Uow;

namespace DayEasy.EntityFramework
{
    public interface IDbContextProvider<out TDbContext>
        where TDbContext : IUnitOfWork
    {
        TDbContext DbContext { get; }
    }
}
