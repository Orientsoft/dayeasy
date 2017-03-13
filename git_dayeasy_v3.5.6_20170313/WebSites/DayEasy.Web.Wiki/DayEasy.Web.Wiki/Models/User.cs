using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Web.Wiki.Models
{
    [Table("TUser")]
    public class User : DEntity<string>
    {
        [Key,StringLength(16)]
        public override string Id { get; set; }

        [Required, StringLength(32)]
        public string Account { get; set; }

        [Required, StringLength(32)]
        public string Password { get; set; }

        [Required, StringLength(16)]
        public string Name { get; set; }

        [Required]
        public int Role { get; set; }
    }
}