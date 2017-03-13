
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.MigrateTools.Migrate
{
    public class ErrorQuestionMigrate : MigrateBase
    {
        private readonly IDayEasyRepository<TP_MarkingDetail> _markingDetailRepository;
        private readonly IDayEasyRepository<TP_ErrorQuestion> _errorQuestionRepository;
        private readonly IDayEasyRepository<TP_Paper> _paperRepository;
        private readonly IDayEasyRepository<TQ_Question> _questionRepository;

        public ErrorQuestionMigrate()
        {
            _markingDetailRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_MarkingDetail>>();
            _errorQuestionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_ErrorQuestion>>();
            _paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();
            _questionRepository = CurrentIocManager.Resolve<IDayEasyRepository<TQ_Question>>();
        }

        public void Check()
        {
            var qids = "errorList.txt".ReadConfig();
            if (!qids.Any())
            {
                Console.WriteLine("没有批次号数据");
                return;
            }
            var list =
                _markingDetailRepository.Where(
                    d => qids.Contains(d.QuestionID) && !(d.IsCorrect.HasValue && d.IsCorrect.Value))
                    .Select(d => new {d.StudentID, d.QuestionID, d.Batch, d.PaperID})
                    .Distinct()
                    .ToList();
            var errors = new List<TP_ErrorQuestion>();
            foreach (var item in list)
            {
                var detail = item;
                if (_errorQuestionRepository.Exists(e => e.StudentID == detail.StudentID
                                                         && e.QuestionID == detail.QuestionID
                                                         && e.Batch == detail.Batch
                                                         && e.PaperID == detail.PaperID))
                    continue;
                var paper = _paperRepository.FirstOrDefault(p => p.Id == detail.PaperID);
                if (paper == null)
                    continue;
                var question = _questionRepository.FirstOrDefault(q => q.Id == detail.QuestionID);
                if (question == null)
                    continue;
                errors.Add(new TP_ErrorQuestion
                {
                    Id = IdHelper.Instance.Guid32,
                    PaperID = detail.PaperID,
                    Batch = detail.Batch,
                    QuestionID = detail.QuestionID,
                    StudentID = detail.StudentID,
                    PaperTitle = paper.PaperTitle,
                    SubjectID = paper.SubjectID,
                    Stage = paper.Stage,
                    QType = question.QType,
                    AddedAt = Clock.Now,
                    Status = (byte) ErrorQuestionStatus.Normal,
                    VariantCount = 0
                });
            }

            var result = _errorQuestionRepository.Insert(errors.ToArray());
            Console.WriteLine(result);
        }
    }
}
