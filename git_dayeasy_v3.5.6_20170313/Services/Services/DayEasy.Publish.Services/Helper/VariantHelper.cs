using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Helper;

namespace DayEasy.Publish.Services.Helper
{
    /// <summary> 变式辅助 </summary>
    internal class VariantHelper
    {
        /// <summary> 添加变式题关系记录 </summary>
        internal static void AddVariantRelation(string qid, List<string> vids)
        {
            List<TQ_VariantRelation>
                inserts = new List<TQ_VariantRelation>(),
                updates = new List<TQ_VariantRelation>();
            var repository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_VariantRelation>>();
            vids.ForEach(vid =>
            {
                var item = repository.FirstOrDefault(v =>
                    (v.QID == qid && v.VID == vid) || (v.QID == vid && v.VID == qid));
                if (item == null)
                {
                    inserts.Add(new TQ_VariantRelation
                    {
                        Id = IdHelper.Instance.GetGuid32(),
                        QID = qid,
                        VID = vid,
                        UseCount = 1
                    });
                }
                else
                {
                    item.UseCount += 1;
                    updates.Add(item);
                }
            });
            if (inserts.Any())
                repository.Insert(inserts);
            if (updates.Any())
                repository.Update(v => new { v.UseCount }, updates.ToArray());
        }
    }
}
