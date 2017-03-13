
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Examination
{
    /// <summary> 班级分析计算参数 </summary>
    public class ClassAnalysisLayerInputDto : DDto
    {
        /// <summary> 考试ID </summary>
        public string ExamId { get; set; }
        /// <summary> A层比例 </summary>
        public decimal LayerA { get; set; }

        /// <summary> B层比例 </summary>
        public decimal LayerB { get; set; }

        /// <summary> C层比例 </summary>
        public decimal LayerC { get; set; }

        /// <summary> D层比例 </summary>
        public decimal LayerD { get; set; }

        /// <summary> E层比例 </summary>
        public decimal LayerE { get; set; }

        public ClassAnalysisLayerInputDto()
        {
            LayerA = 10M;
            LayerB = 30M;
            LayerC = 30M;
            LayerD = 20M;
            LayerE = 10M;
        }
    }
}
