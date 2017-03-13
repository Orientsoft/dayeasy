using DayEasy.Core.Domain.Entities;

namespace DayEasy.Application.Services.Dto
{
    /// <summary> 作业中心 & 批阅中心 基础实体 </summary>
    public class VWorkDto : DDto
    {
        public string PaperId { get; set; }
        public string Batch { get; set; }
        public string PaperTitle { get; set; }
        public byte PublishType { get; set; }
        public bool IsAb { get; set; }
        public string ClassId { get; set; }
        public string ClassName { get; set; }
        public int Step { get; set; }
        public decimal Score { get; set; }
        public decimal ScoreA { get; set; }
        public decimal ScoreB { get; set; }
        public string ColleagueId { get; set; }

        public VWorkDto(string batch, string paperId, int step = 0)
        {
            Batch = batch;
            PaperId = paperId;
            Step = step;
        }
    }
}