using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;
using DayEasy.Utility.Extend;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary>
    /// 答卷Detail
    /// </summary>
    [AutoMapFrom(typeof(TP_MarkingDetail))]
    public class MkDetailDto : DDto
    {
        public string Id { get; set; }
        [MapFrom("MarkingID")]
        public string MarkingId { get; set; }
        [MapFrom("QuestionID")]
        public string QuestionId { get; set; }
        [MapFrom("SmallQID")]
        public string SmallQuestionId { get; set; }
        [MapFrom("AnswerIDs")]
        public string AnswerIds { get; set; }
        public string[] AnswerIdList { get; set; }
        public int[] AnswerIndexs { get; set; }
        public string AnswerContent { get; set; }
        public string AnswerImages { get; set; }
        public string[] AnswerImageList
        {
            get
            {
                return string.IsNullOrWhiteSpace(AnswerImages)
                ? new string[] { }
                : AnswerImages.JsonToObject<string[]>();
            }
            set { AnswerImages = (value == null ? null : value.ToJson()); }
        }
        public decimal Score { get; set; }
        public bool? IsCorrect { get; set; }
        public bool IsFinished { get; set; }
        /// <summary>
        /// 问题得分,若为0，则该问满分
        /// </summary>
        public decimal? CurrentScore { get; set; }
        /// <summary>
        /// 教师评语
        /// </summary>
        public string TeacherWords { get; set; }
    }

    public class MarkedDetailDto : DDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set;}
        public string QuestionId { get; set; }
        public string SmallQId { get; set; }
        public string AnswerContent { get; set; }
        public string AnswerImages { get; set; }
        public string[] AnswerImageList
        {
            get
            {
                return string.IsNullOrWhiteSpace(AnswerImages)
                ? new string[] { }
                : AnswerImages.JsonToObject<string[]>();
            }
            set { AnswerImages = (value == null ? null : value.ToJson()); }
        }
        public bool IsFinished { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal CurrentScore { get; set; }
    }

    public class MkDetailMinDto : DDto
    {
        public string Id { get; set; }
        public string QuestionId { get; set; }
        public bool? IsCorrect { get; set; }
        public decimal Score { get; set; }
    }

}
