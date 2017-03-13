using DayEasy.Core.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DayEasy.Contracts.Models
{
    public class TP_MarkingPicture : DEntity<string>
    {
        [Key]
        [Column("ID")]
        public override string Id { get; set; }

        [StringLength(32)]
        public string BatchNo { get; set; }

        [Required]
        [StringLength(32)]
        public string PaperID { get; set; }

        [StringLength(32)]
        public string ClassID { get; set; }

        public long StudentID { get; set; }

        [StringLength(16)]
        public string StudentName { get; set; }
        public string AnswerImgUrl { get; set; }
        public byte AnswerImgType { get; set; }
        public string SheetAnswers { get; set; }
        public bool IsSuccess { get; set; }
        public string RightAndWrong { get; set; }
        public string Marks { get; set; }
        public int SubmitSort { get; set; }
        public long AddedBy { get; set; }
        public DateTime AddedAt { get; set; }
        /// <summary> ״̬ </summary>
        public byte Status { get; set; }
        /// <summary> ��ҳ�� </summary>
        public int TotalPageNum { get; set; }
        /// <summary> �Ƿ��ǵ��� </summary>
        public bool? IsSingleFace { get; set; }
        /// <summary> �������ʱ�� </summary>
        public DateTime? LastMarkingTime { get; set; }

        /// <summary> �͹۴������ </summary>
        [StringLength(512)]
        public string ObjectiveError { get; set; }
        /// <summary> �͹���÷� </summary>
        public decimal? ObjectiveScore { get; set; }
    }
}
