using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TQ_Analysis : DEntity<string>
    {
        [Key]
        [Column("AnalysisID")]
        public override string Id { get; set; }
        public string QID { get; set; }
        public string AnalysisContent { get; set; }
        public string AnalysisImage { get; set; }
    }
}
