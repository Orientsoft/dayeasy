using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TS_Area : DEntity
    {
        [Key]
        [Column("Code")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public override int Id { get; set; }
        public string Name { get; set; }
        public int ParentCode { get; set; }
        public string FullPinYin { get; set; }
        public string SimplePinYin { get; set; }
        public int Sort { get; set; }
    }
}
