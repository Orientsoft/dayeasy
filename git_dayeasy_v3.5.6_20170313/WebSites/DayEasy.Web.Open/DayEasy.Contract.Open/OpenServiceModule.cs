using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Dtos.Variant;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Core.Modules;
using DayEasy.Models.Open.Group;
using DayEasy.Models.Open.Paper;
using DayEasy.Models.Open.System;
using DayEasy.Models.Open.User;
using DayEasy.Models.Open.Work;

namespace DayEasy.Contract.Open
{
    [DependsOn(typeof(CoreModule))]
    public class OpenServiceModule:DModule
    {
        public override void Initialize()
        {
            AutoMapperHelper.CreateMapper<TS_Area, MAreaDto>();
            AutoMapperHelper.CreateMapper<KnowledgeDto, MKnowledgeDto>();
            AutoMapperHelper.CreateMapper<UserDto, MUserDto>();
            AutoMapperHelper.CreateMapper<GroupDto, MGroupDto>();
            AutoMapperHelper.CreateMapper<MemberDto, MMemberDto>();
            //Question
            AutoMapperHelper.CreateMapper<AnswerDto, MAnswerDto>();
            AutoMapperHelper.CreateMapper<SmallQuestionDto, MDetailDto>();
            AutoMapperHelper.CreateMapper<QuestionDto, MQuestionDto>();

            //Paper
            AutoMapperHelper.CreateMapper<PaperDto, MPaperDto>();
            AutoMapperHelper.CreateMapper<PaperQuestionDto, MPaperQuestionDto>();
            AutoMapperHelper.CreateMapper<PaperSectionDto, MPaperSectionDto>();
            AutoMapperHelper.CreateMapper<QuestionDto, MPaperQuestionDto>();

            AutoMapperHelper.CreateMapper<JointGroupDto, MJointUsageDto>();

            AutoMapperHelper.CreateMapper<MDynamicSearchDto, DynamicSearchDto>();
            AutoMapperHelper.CreateMapper<MDynamicSearchDto, DynamicSearchDto>();

            AutoMapperHelper.CreateMapper<QuestionVariantDto, MQuestionVariantDto>();

            base.Initialize();
        }
    }
}
