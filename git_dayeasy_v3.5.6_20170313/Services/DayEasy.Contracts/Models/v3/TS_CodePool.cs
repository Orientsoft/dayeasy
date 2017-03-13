using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> 序号池 </summary>
    public class TS_CodePool : DEntity<int>
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        /// <summary> 序号类型 </summary>
        public byte Type { get; set; }

        /// <summary> 序号 </summary>
        public long Code { get; set; }

        /// <summary> 序号等级 </summary>
        public byte Level { get; set; }
    }
}
