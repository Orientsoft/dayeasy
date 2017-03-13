using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Marking
{
    /// <summary> 学生答卷图片 </summary>
    [AutoMap(typeof(TP_MarkingPicture))]
    public class MkPictureDto : DDto
    {
        [MapFrom("Id")]
        public string PictureId { get; set; }
        public string JointPictureId { get; set; }
        public long StudentId { get; set; }
        public string StudentName { get; set; }
        [MapFrom("RightAndWrong")]
        public string Icons { get; set; }
        public string Marks { get; set; }
        [MapFrom("AnswerImgUrl")]
        public string PictureUrl { get; set; }
        public string ObjectiveError { get; set; }
        /// <summary> 客观题得分 </summary>
        public decimal? ObjectiveScore { get; set; }
        public decimal TotalScore { get; set; }
        public List<MkDetailMinDto> Details { get; set; }
        [MapFrom("LastMarkingTime")]
        public DateTime? MarkingTime { get; set; }
        public bool IsMarking
        {
            get { return MarkingTime.HasValue; }
        }
        public int UnMarkingCount { get; set; }
        public int? ExceptionType { get; set; }
    }

    [DataContract(Name = "marking_question_area")]
    public class MkQuestionAreaDto : DDto
    {
        [DataMember(Name = "x", Order = 1)]
        public float X { get; set; }
        [DataMember(Name = "y", Order = 2)]
        public float Y { get; set; }
        [DataMember(Name = "width", Order = 3)]
        public float Width { get; set; }
        [DataMember(Name = "height", Order = 4)]
        public float Height { get; set; }

        [DataMember(Name = "id", Order = 5)]
        public string Id { get; set; }

        [DataMember(Name = "index", Order = 6)]
        public string Index { get; set; }

        //是否为填空题
        [DataMember(Name = "t", Order = 7)]
        public bool T { get; set; }
    }


    /// <summary> 表情或评语标记 </summary>
    [DataContract(Name = "comment")]
    public class MkComment : MkSymbolBase
    {
        [DataMember(Name = "w", Order = 4)]
        public string Words { get; set; }
    }

    [DataContract(Name = "symbol_base")]
    public class MkSymbolBase : DDto
    {
        [DataMember(Name = "x", Order = 1)]
        public float X { get; set; }

        [DataMember(Name = "y", Order = 2)]
        public float Y { get; set; }

        /// <summary>
        ///枚举 SymbolType
        /// </summary>
        [DataMember(Name = "t", Order = 3)]
        public int SymbolType { get; set; }
    }


    [DataContract(Name = "symbol")]
    public class MkSymbol : MkSymbolBase
    {
        /// <summary>
        /// 问题分数
        /// </summary>
        [DataMember(Name = "w", Order = 4)]
        public double Score { get; set; }

        /// <summary>
        /// 问题ID
        /// </summary>
        [DataMember(Name = "id", Order = 5)]
        public string QuestionId { get; set; }
    }

}
