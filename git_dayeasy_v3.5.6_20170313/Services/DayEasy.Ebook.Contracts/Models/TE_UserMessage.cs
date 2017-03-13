using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Ebook.Contracts.Models
{
    public class TE_UserMessage : DEntity<string>
    {
        [Key]
        [Column("MessageId")]
        public override string Id { get; set; }
        [NotMapped]
        public string MessageId { get { return Id; } }
        public byte MessageType { get; set; }
        public string MessageContent { get; set; }
        public long SenderID { get; set; }
        public string ReceiverID { get; set; }
        public System.DateTime SendTime { get; set; }
        public byte Status { get; set; }
    }
}
