
using System;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 班级分析 - 重点率分析 </summary>
    public class ClassAnalysisKeyDto : AnalysisDto
    {
        /// <summary> 平均分 </summary>
        public decimal AverageScore { get; set; }

        /// <summary> 平均分差值 </summary>
        public decimal AverageScoreDiff { get; set; }

        /// <summary> 均值比 </summary>
        public decimal AverageRatio { get; set; }

        /// <summary> 均值比差值 </summary>
        public decimal AverageRatioDiff
        {
            get { return AverageRatio > 0 ? AverageRatio - 1M : 0M; }
        }

        /// <summary> 重点率上线人数 </summary>
        public int KeyCount { get; set; }

        /// <summary> 班级重点率 </summary>
        public decimal KeyScale
        {
            get { return StudentCount <= 0 ? 0M : Math.Round(KeyCount/(decimal) StudentCount, 4); }
        }

        /// <summary> 重点率差值 </summary>
        public decimal KeyScaleDiff { get; set; }

        /// <summary> A卷合格人数 </summary>
        public int ACount { get; set; }

        /// <summary> 合格率 </summary>
        public decimal AScale
        {
            get { return StudentCount <= 0 ? 0M : Math.Round(ACount/(decimal) StudentCount, 4); }
        }

        /// <summary> 合格率差值 </summary>
        public decimal AScaleDiff { get; set; }

        /// <summary> A卷不合格人数 </summary>
        public int UnACount { get; set; }

        /// <summary> 不合格率 </summary>
        public decimal UnAScale
        {
            get { return StudentCount <= 0 ? 0M : Math.Round(UnACount/(decimal) StudentCount, 4); }
        }

        /// <summary> 不合格率差值 </summary>
        public decimal UnAScaleDiff { get; set; }
    }

    /// <summary> 班级分析 - 分层 </summary>
    public class ClassAnalysisLayerDto : AnalysisDto
    {
        /// <summary> A层人数 </summary>
        public int LayerA { get; set; }

        /// <summary> B层人数 </summary>
        public int LayerB { get; set; }

        /// <summary> C层人数 </summary>
        public int LayerC { get; set; }

        /// <summary> D层人数 </summary>
        public int LayerD { get; set; }

        /// <summary> E层人数 </summary>
        public int LayerE { get; set; }
    }
}
