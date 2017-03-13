
using DayEasy.Contract.Open.Contracts;
using DayEasy.EntityFramework;
using DayEasy.Services;

namespace DayEasy.Contract.Open.Services
{
    public partial class OpenService : DayEasyService, IOpenContract
    {
        public OpenService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        {
        }
    }
}
