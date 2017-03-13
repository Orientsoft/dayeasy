using DayEasy.Core;
using DayEasy.EntityFramework;

namespace DayEasy.Services
{
    public abstract class DayEasyService : DService
    {
        protected DayEasyService(IDbContextProvider<DayEasyDbContext> context)
            : base(context.DbContext)
        {
        }
    }
}
