﻿
namespace DayEasy.ThirdPlatform.Entity.Result
{
    /// <summary> 腾讯QQ返回原始数据 </summary>
    internal class TencentResult
    {
        public int ret { get; set; }
        public string msg { get; set; }
        public int is_lost { get; set; }
        public string nickname { get; set; }
        public string gender { get; set; }
        public string province { get; set; }
        public string city { get; set; }
        public string year { get; set; }
        public string figureurl { get; set; }
        public string figureurl_1 { get; set; }
        public string figureurl_2 { get; set; }
        public string figureurl_qq_1 { get; set; }
        public string figureurl_qq_2 { get; set; }
        public string is_yellow_vip { get; set; }
        public string vip { get; set; }
        public string yellow_vip_level { get; set; }
        public string level { get; set; }
        public string is_yellow_year_vip { get; set; }
    }
}
