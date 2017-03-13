using System.Collections.Generic;
using DayEasy.Contracts.Dtos;
using DayEasy.Core;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 图文广告相关契约 </summary>
    public partial interface IAdvertContract : IDependency
    {
        /// <summary> 查询图文广告列表 </summary>
        /// <param name="index"></param>
        /// <param name="size"></param>
        /// <param name="category">分类</param>
        /// <param name="key">关键字</param>
        /// <returns></returns>
        DResults<AdvertDto> Adverts(int index, int size, string category = "", string key = "");

        /// <summary> 根据ID集合查询图文广告 </summary>
        List<AdvertDto> Adverts(List<string> ids);

        /// <summary> 查询图文广告 </summary>
        DResult<AdvertDto> Advert(string id);

        /// <summary> 新增、修改 </summary>
        DResult<AdvertDto> Edit(AdvertDto dto);

        /// <summary> 删除 </summary>
        DResult Delete(string id);

    }
}
