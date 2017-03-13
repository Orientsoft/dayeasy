using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_MangagerHistory : DEntity<string>
    {
        [Key]
        [Column("HistoryID")]
        public override string Id { get; set; }
        public long ManagerID { get; set; }
        public string ObjID { get; set; }
        public byte ObjType { get; set; }
        public System.DateTime AddTime { get; set; }
    }
}
