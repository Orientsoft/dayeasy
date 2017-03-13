
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Core.Domain
{
    /// <summary> 通用分页对象 </summary>
    public class DPage : DDto
    {
        private const int MaxPage = 1000;
        private const int MaxSize = 500;
        /// <summary> 页码,从0开始 </summary>
        public int Page { get; set; }
        /// <summary> 每页显示数 </summary>
        public int Size { get; set; }

        protected DPage(int page = 0, int size = 15)
        {
            if (page < 0) page = 0;
            if (page > MaxPage) page = MaxPage;
            if (size < 1) size = 1;
            if (size > MaxSize) size = MaxSize;
            Page = page;
            Size = size;
        }

        /// <summary> 新建分页 </summary>
        /// <param name="page">页码，从0开始计数</param>
        /// <param name="size">每页显示数，最大500</param>
        /// <returns></returns>
        public static DPage NewPage(int page = 0, int size = 15)
        {
            return new DPage(page, size);
        }
    }
}
