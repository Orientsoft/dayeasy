using DayEasy.Core;
using DayEasy.EntityFramework;

namespace DayEasy.Services
{
    public class Version3Service : DService
    {
        public Version3Service(IDbContextProvider<Version3DbContext> context)
            : base(context.DbContext)
        {
        }
    }
}
