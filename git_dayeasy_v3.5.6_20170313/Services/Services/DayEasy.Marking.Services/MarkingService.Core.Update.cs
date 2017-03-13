
using System;
using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.Marking;
using DayEasy.Contracts.Models;
using DayEasy.Core;
using DayEasy.Marking.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Timing;

namespace DayEasy.Marking.Services
{
    /// <summary> 批阅业务处理 </summary>
    public partial class MarkingService
    {

        #region 扫描打印阅卷

        /// <summary> 扫描打印阅卷 </summary>
        /// <returns></returns>
        private DResult PrintMarking(List<MkDetailDto> details, long teacherId,TP_MarkingPicture picture)
        {
            var list = new List<TP_MarkingDetail>();

            if (details != null && details.Any())
            {
                //重置客观题答案
                ResetObjectiveAnswers(details);
                foreach (var detail in details)
                {
                    var id = detail.QuestionId;
                    var smallId = detail.SmallQuestionId;
                    var hasSmall = !string.IsNullOrWhiteSpace(smallId);
                    var item = MarkingDetailRepository.FirstOrDefault(d =>
                        d.Batch == picture.BatchNo && d.PaperID == picture.PaperID &&
                        d.StudentID == picture.StudentID && d.QuestionID == id &&
                        (!hasSmall || d.SmallQID == smallId));
                    if (item == null) 
                        continue;
                    //item.IsFinished = true;
                    item.IsCorrect = detail.IsCorrect;
                    if (detail.IsCorrect.HasValue && detail.IsCorrect.Value)
                        item.CurrentScore = item.Score;
                    else
                        item.CurrentScore = detail.CurrentScore ?? 0M;
                    item.MarkingBy = teacherId;
                    item.MarkingAt = Clock.Now;
                    item.AnswerIDs = detail.AnswerIds.ToJson();
                    item.AnswerContent = detail.AnswerContent;
                    list.Add(item);
                }
            }
            
            return UpdateMarking(list,teacherId, picture) > 0
                ? MarkingConsts.Success
                : DResult.Error(MarkingConsts.MsgMarkingError);
        }

        #endregion

        /// <summary> 重置客观题答案 - (错误的->正确) </summary>
        private void ResetObjectiveAnswers(List<MkDetailDto> details)
        {
            if (details == null || !details.Any()) return;
            foreach (var detail in details)
            {
                if (!(detail.IsCorrect.HasValue && detail.IsCorrect.Value)) continue;
                var qid = detail.QuestionId;
                var qItem = QuestionRepository.FirstOrDefault(q => q.Id == qid);
                //客观题
                if (qItem == null || !qItem.IsObjective) continue;
                //设置正确答案
                var id = qid;
                if (!string.IsNullOrWhiteSpace(detail.SmallQuestionId))
                    id = detail.SmallQuestionId;
                var rights = AnswerRepository.Where(a => a.QuestionID == id && a.IsCorrect)
                    .OrderBy(a => a.Sort)
                    .Select(a => new {a.Id, a.Sort});
                detail.AnswerIdList = rights.Select(t => t.Id).ToArray();
                detail.AnswerContent = rights.Select(t => t.Sort).ToArray()
                    .Aggregate(string.Empty, (c, t) => c + Consts.OptionWords[t]);
            }
        }
    
    }
}
