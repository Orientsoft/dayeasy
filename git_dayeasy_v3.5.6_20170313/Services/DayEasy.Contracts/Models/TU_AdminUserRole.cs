using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_AdminUserRole : DEntity
    {
        [Key]
        [DatabaseGenerated((DatabaseGeneratedOption.Identity))]
        public override int Id { get; set; }
        public long UserId { get; set; }
        public long UserRole { get; set; }
    }
}
