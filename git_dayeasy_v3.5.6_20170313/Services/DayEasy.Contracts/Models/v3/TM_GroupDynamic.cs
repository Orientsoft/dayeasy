using System;
using System.ComponentModel.DataAnnotations;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    /// <summary> È¦×Ó¶¯Ì¬ </summary>
    public class TM_GroupDynamic : DEntity<string>
    {
        [Key]
        [StringLength(32)]
        public override string Id { get; set; }

        [Required]
        [StringLength(32)]
        public string GroupId { get; set; }

        public byte DynamicType { get; set; }

        public byte? ContentType { get; set; }

        [StringLength(32)]
        public string ContentId { get; set; }

        [StringLength(128)]
        public string Title { get; set; }

        [StringLength(512)]
        public string Message { get; set; }

        [StringLength(2048)]
        public string ReceiverIds { get; set; }

        public int ReceiveRole { get; set; }

        public int GoodCount { get; set; }

        [StringLength(2048)]
        public string Goods { get; set; }

        public int CommentCount { get; set; }

        public byte Status { get; set; }

        public long AddedBy { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
