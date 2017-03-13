using Novacode;
using System.Collections.Generic;

namespace DayEasy.Office.Models
{
    //导出Word的试卷实体类
    public class WdPaper
    {
        public string Num { get; set; }
        public string Title { get; set; }
        public decimal Score { get; set; }
        public int SubjectId { get; set; }
        public List<WdSection> Sections { get; set; }
        /// <summary> 简化下载内容 </summary>
        public bool Simplify { get; set; }
    }

    public class WdSection
    {
        public int PSectionType { get; set; }
        public int Sort { get; set; }
        //题型
        public int Type { get; set; }
        public string Name { get; set; }
        public List<WdQuestion> Questions { get; set; }
        /// <summary> 小问通排 </summary>
        public bool SmallRow { get; set; }
    }

    public class WdQuestionBase
    {
        public int Sort { get; set; }
        public string Body { get; set; }
        public decimal Score { get; set; }
        public int Type { get; set; }
        public bool IsObjective { get; set; }
        public bool ShowOption { get; set; }
        public string[] Images { get; set; }
        public List<WdAnswer> Answers { get; set; }
        /// <summary>
        /// 公式及img标签图片
        /// </summary>
        public List<string> Keys { get; set; }
        /// <summary>
        /// 数据库字段存储图片
        /// </summary>
        public List<string> ImgKeys { get; set; }
    }

    public class WdQuestion : WdQuestionBase
    {
        public List<WdSmallQuestion> SmallQuestions { get; set; }
    }

    public class WdSmallQuestion : WdQuestionBase
    {

    }

    public class WdAnswer
    {
        public int Sort { get; set; }
        public string Tag { get; set; }
        public string Body { get; set; }
        public bool IsCorrect { get; set; }
        public string[] Images { get; set; }
        public List<string> Keys { get; set; }
        public List<string> ImgKeys { get; set; }
    }

    /// 分科目导出问题列表
    public class WdQuestionGroup
    {
        public string Title { get; set; }
        public List<WdSubject> Subjects { get; set; }
    }

    public class WdSubject
    {
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public List<WdSection> Sections { get; set; }
    }

    public class WdBodyTable
    {
        public string Body { get; set; }
        public List<Table> Tables { get; set; }
    }
}
