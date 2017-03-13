using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_ThirdPlatform : DEntity<string>
    {
        [Key]
        public override string Id { get; set; }
        public long UserID { get; set; }
        public byte PlatformType { get; set; }
        public string PlatformId { get; set; }
        public string AccessToken { get; set; }
        public string Nick { get; set; }
        public string Profile { get; set; }
        public System.DateTime AddedAt { get; set; }
    }
}
