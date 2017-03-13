namespace DayEasy.Office.Enum
{
    public enum DowloadType : byte
    {
        /// <summary>
        /// A卷或普通卷
        /// </summary>
        PaperA = 2,

        /// <summary>
        /// B卷
        /// </summary>
        PaperB = 4,

        /// <summary>
        /// 答题卡
        /// </summary>
        Card = 8,

        /// <summary>
        /// 原稿
        /// </summary>
        Original = 16,

        /// <summary>
        /// 作业
        /// </summary>
        Work = 32,

        /// <summary>
        /// 参考答案
        /// </summary>
        Answer = 64,

        /// <summary>
        /// 帮助文档
        /// </summary>
        Help = 128
    }
}
