using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayEasy.Paper.Services.Model
{
    /// <summary>
    /// 自动出卷条件（从前台用户设置而来）
    /// </summary>
    public class AutoCondition
    {
        public List<AutoKp> Kps { get; set; }
        public List<AutoType> Qtypes { get; set; }
        public int Diffic { get; set; }
    }

    /// <summary>
    /// 用户设置知识点
    /// </summary>
    public class AutoKp
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// 用户设置题型
    /// </summary>
    public class AutoType
    {
        public int Type { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string PaperSectionType { get; set; }
    }

    /// <summary>
    /// 试卷属性
    /// </summary>
    public class PaperProperty
    {
        /// <summary>
        /// 题型
        /// </summary>
        public int QType { get; set; }
        /// <summary>
        /// 题型数量
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 知识点
        /// </summary>
        public List<string> Points { get; set; }
        /// <summary>
        /// 难度系数
        /// </summary>
        public List<double> Difficulties { get; set; }
    }
}
