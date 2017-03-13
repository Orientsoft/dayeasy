using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace DayEasy.Marking.Services.Helper
{
    internal static class MarkingHelper
    {
        internal static DRegion CombineRegion(params MkQuestionAreaDto[] areas)
        {
            if (areas.IsNullOrEmpty())
                return new DRegion();
            var rects = areas.Select(t => new RectangleF(t.X, t.Y, t.Width, t.Height))
                .ToList();
            var rect = ImageCls.CombineRegion(rects, 20);
            return new DRegion(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Task UpdateRegionAsync(string joint, byte type, string mark)
        {
            return Task.Factory.StartNew(() =>
            {
                JointHelper.ResetAreaCache(joint);
                //                var area = JsonHelper.JsonList<MkQuestionAreaDto>(mark).ToList();
                //                if (area.IsNullOrEmpty())
                //                    return;
                //                if (type == 0) type = 1;
                //                var bagRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_JointBag>>();
                //                var bags = bagRepository.Where(t => t.JointBatch == joint && t.SectionType == type).ToList();
                //                if (!bags.Any())
                //                    return;
                //                foreach (var bag in bags)
                //                {
                //                    var qids = JsonHelper.JsonList<string>(bag.QuestionIds);
                //                    var sorts = area.Where(t => qids.Contains(t.Id)).ToArray();
                //                    var region = CombineRegion(sorts);
                //                    bag.Region = JsonHelper.ToJson(region, NamingType.CamelCase);
                //                }
                //                bagRepository.Update(t => new
                //                {
                //                    t.Region
                //                }, bags.ToArray());
            });
        }
    }
}
