using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Models
{
    public class TU_UserEx : DEntity<long>
    {
        [Key]
        [ForeignKey("User")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("UserID")]
        public override long Id { get; set; }
        public string IDCard { get; set; }
        public string IDCardFront { get; set; }
        public string IDCardBack { get; set; }
        public string CardNo { get; set; }
        public string CardFront { get; set; }
        public string CardBack { get; set; }
        public Nullable<System.DateTime> IDCardApplyTime { get; set; }
        public Nullable<System.DateTime> IDCardCheckPassTime { get; set; }
        public Nullable<byte> IDCardStatus { get; set; }
        public Nullable<System.DateTime> CardApplyTime { get; set; }
        public Nullable<System.DateTime> CardCheckPassTime { get; set; }
        public Nullable<byte> CardStatus { get; set; }
        public string CheckBy { get; set; }
        public Nullable<int> SubjectID { get; set; }

        public virtual TU_User User { get; set; }
    }
}
