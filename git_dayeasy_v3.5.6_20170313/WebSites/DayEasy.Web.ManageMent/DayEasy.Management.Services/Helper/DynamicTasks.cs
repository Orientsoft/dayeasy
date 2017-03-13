using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using System.Threading.Tasks;

namespace DayEasy.Management.Services.Helper
{
    public class DynamicTasks
    {
        public static Task Delete(string contentId)
        {
            return UpdateStatus(contentId, NormalStatus.Delete);
        }
        public static Task UpdateStatus(string contentId, NormalStatus status)
        {
            return Task.Factory.StartNew(() =>
            {
                var dynamicRepository = CurrentIocManager.Resolve<IVersion3Repository<TM_GroupDynamic>>();
                dynamicRepository.Update(new TM_GroupDynamic { Status = (byte)status }, d => d.ContentId == contentId,
                    "Status");
            });
        }
    }
}
