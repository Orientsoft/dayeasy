using DayEasy.Contracts.Dtos;
using DayEasy.Utility;

namespace DayEasy.Contracts
{
    /// <summary> 图文广告分类相关契约 </summary>
    public partial interface IAdvertContract
    {
        /// <summary> 分类列表 </summary>
        DResults<AdvertCategoryDto> Categorys();
        
        /// <summary> 新增、修改分类 </summary>
        DResult<AdvertCategoryDto> CategoryEdit(AdvertCategoryDto dto);

        /// <summary> 删除分类 </summary>
        DResult CategoryDelete(string id);

    }
}
