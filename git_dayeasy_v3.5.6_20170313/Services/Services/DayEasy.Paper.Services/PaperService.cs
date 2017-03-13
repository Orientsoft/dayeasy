using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Paper;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.EntityFramework;
using DayEasy.Paper.Services.Helper;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;
using System.Collections.Generic;

namespace DayEasy.Paper.Services
{
    public partial class PaperService : DayEasyService, IPaperContract
    {
        private readonly ILogger _logger = LogManager.Logger<PaperService>();
        public PaperService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        { }

        #region 试卷缓存
        private static PaperDto ParsePaperDto(TP_Paper paper)
        {
            if (paper == null)
                return null;
            var dto = paper.MapTo<PaperDto>();
            var user =
                CurrentIocManager.Resolve<IDayEasyRepository<TU_User, long>>()
                    .SingleOrDefault(u => u.Id == paper.AddedBy);
            dto.UserName = user == null ? string.Empty : user.TrueName;

            dto.Kps = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(paper.KnowledgeIDs))
                dto.Kps = paper.KnowledgeIDs.JsonToObject<Dictionary<string, string>>();

            dto.Tags = new List<string>();
            if (!string.IsNullOrWhiteSpace(paper.TagIDs))
                dto.Tags = paper.TagIDs.JsonToObject<List<string>>();
            dto.PaperScores = (paper.PaperScores ?? string.Empty).JsonToObject<PaperScoresDto>();
            return dto;
        }
        #endregion

        #region 加载问题详情

        /// <summary>
        /// 加载问题详情
        /// </summary>
        /// <param name="paperModel"></param>
        /// <param name="loadQuestion"></param>
        /// <returns></returns>
        private PaperDetailDto MakePaperDetails(TP_Paper paperModel, bool loadQuestion = true)
        {
            var paperDto = ParsePaperDto(paperModel);

            var vpPaper = new PaperDetailDto
            {
                PaperBaseInfo = paperDto,
                PaperSections = null
            };

            if (!loadQuestion)
                return vpPaper;
            vpPaper.LoadQuestions(paperDto.SortType());

            return vpPaper;
        }

        #endregion

    }
}
