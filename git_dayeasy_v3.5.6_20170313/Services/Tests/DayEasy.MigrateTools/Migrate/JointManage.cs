using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Models;
using DayEasy.Office;
using DayEasy.Services;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Logging;

namespace DayEasy.MigrateTools.Migrate
{
    public class JointManage : MigrateBase
    {
        private readonly ILogger _logger = LogManager.Logger<JointPicture>();
        private readonly IDayEasyRepository<TP_JointMarking, string> _jointMarkingRepository;
        private readonly IDayEasyRepository<TP_MarkingDetail, string> _detailRepository;
        private readonly IDayEasyRepository<TC_Usage, string> _usageRepository;
        private readonly IDayEasyRepository<TU_User, long> _userRepository;
        private readonly IDayEasyRepository<TP_Paper, string> _paperRepository;
        private readonly IVersion3Repository<TG_Group, string> _groupRepository;
        private readonly IPaperContract _paperContract;

        public JointManage()
        {
            _jointMarkingRepository = Container.Resolve<IDayEasyRepository<TP_JointMarking, string>>();
            _usageRepository = Container.Resolve<IDayEasyRepository<TC_Usage, string>>();
            _detailRepository = Container.Resolve<IDayEasyRepository<TP_MarkingDetail, string>>();
            _userRepository = Container.Resolve<IDayEasyRepository<TU_User, long>>();
            _paperRepository = Container.Resolve<IDayEasyRepository<TP_Paper, string>>();
            _groupRepository = Container.Resolve<IVersion3Repository<TG_Group, string>>();
            _paperContract = Container.Resolve<IPaperContract>();
        }

        public void ExportDetail()
        {
            var jointBatchs = new List<string>();
            const string savePath = @"D:\\JointManage\";

            try
            {
                using (var reader = new StreamReader(savePath + "batch.txt", Encoding.Default))
                {
                    var batch = reader.ReadLine();
                    while (batch.IsNotNullOrEmpty())
                    {
                        jointBatchs.Add(batch);
                        batch = reader.ReadLine();
                    }
                }
                jointBatchs.ForEach(jointBatch =>
                {
                    var joint = _jointMarkingRepository.Load(jointBatch);
                    var teacher = _userRepository.FirstOrDefault(u => u.Id == joint.AddedBy);
                    var paper = _paperRepository.FirstOrDefault(p => p.Id == joint.PaperId);

                    var usages = _usageRepository.Where(u => u.JointBatch == jointBatch)
                        .Select(u => new {u.Id, u.ClassId}).ToList();

                    var groupIds = usages.Select(u => u.ClassId).ToList();
                    var groups = _groupRepository.Where(g => groupIds.Contains(g.Id))
                        .Select(g => new {g.Id, g.GroupName}).ToList();

                    var batchs = usages.Select(u => u.Id).ToList();
                    var details = _detailRepository.Where(d => batchs.Contains(d.Batch)).OrderBy(d=>d.Batch)
                        .Select(d => new {d.StudentID, d.QuestionID, d.SmallQID, d.Batch, d.CurrentScore}).ToList();

                    var userIds = details.Select(d => d.StudentID).Distinct().ToList();
                    var users = _userRepository.Where(u => userIds.Contains(u.Id))
                        .Select(u => new {u.Id, u.TrueName}).ToList();

                    var paperResult = _paperContract.PaperDetailById(joint.PaperId);
                    if (!paperResult.Status || paperResult.Data == null) return;
                    var sections = paperResult.Data.PaperSections;

                    var list = new List<JointScoreDetailDto>();

                    userIds.ForEach(uid =>
                    {
                        var dto = new JointScoreDetailDto
                        {
                            Scores = new List<object>()
                        };
                        var user = users.FirstOrDefault(u => u.Id == uid);
                        if (user != null) dto.StudentName = user.TrueName;

                        var detail = details.FirstOrDefault(d => d.StudentID == uid);
                        if (detail != null)
                        {
                            var batch = usages.FirstOrDefault(u => u.Id == detail.Batch);
                            if (batch != null)
                            {
                                var group = groups.FirstOrDefault(g => g.Id == batch.ClassId);
                                if (group != null) dto.GroupName = group.GroupName;
                            }
                        }
                        sections.ForEach(s => s.Questions.ForEach(q =>
                        {
                            if (q.Question.IsObjective && q.Question.HasSmall)
                            {
                                q.Question.Details.ForEach(d =>
                                {
                                    var score = details.Where(i =>
                                        i.StudentID == uid && i.QuestionID == q.Question.Id &&
                                        i.SmallQID == d.Id)
                                        .Sum(i => i.CurrentScore);
                                    dto.Scores.Add(score);
                                });
                            }
                            else
                            {
                                var score = details.Where(i =>
                                    i.StudentID == uid && i.QuestionID == q.Question.Id)
                                    .Sum(i => i.CurrentScore);
                                dto.Scores.Add(score);
                            }
                        }));

                        list.Add(dto);
                    });

                    var fileName = teacher.TrueName + "-" + paper.PaperTitle + "[" + paper.PaperNo + "].xls";

                    var ds = new DataSet();
                    var dt = new DataTable();
                    var titleList = new List<object> {"姓名", "班级"};

                    dt.Columns.Add("UserName", typeof (string));
                    dt.Columns.Add("GroupName", typeof (string));
                    var sort = 1;
                    sections.ForEach(s=>s.Questions.ForEach(q =>
                    {
                        if (q.Question.IsObjective && q.Question.HasSmall)
                        {
                            q.Question.Details.ForEach(d =>
                            {
                                dt.Columns.Add("Sort" + (sort++), typeof(string));
                                var title = s.Description + " " + d.Sort + "题";
                                titleList.Add(title);
                            });
                        }
                        else
                        {
                            dt.Columns.Add("Sort" + (sort++), typeof(string));
                            var title = s.Description + " " + q.Sort + "题";
                            titleList.Add(title);
                        }
                    }));
                    dt.Rows.Add(titleList.ToArray());

                    list.ForEach(d =>
                    {
                        var tmp = new List<object> {d.StudentName, d.GroupName};
                        tmp.AddRange(d.Scores);
                        dt.Rows.Add(tmp.ToArray());
                    });

                    ds.Tables.Add(dt);
                    ExcelHelper.Export(ds, fileName, savePath);
                    Console.WriteLine("成功导出{0}！", jointBatch);
                });
                Console.WriteLine("导出完成！");
            }
            catch (Exception ex)
            {
                Console.WriteLine("异常: {0}", ex.Message);
                _logger.Error(ex.Message, ex);
            }
        }
    }

    public class JointScoreDetailDto
    {
        public string GroupName { get; set; }
        public string StudentName { get; set; }
        public List<object> Scores { get; set; }
    }

    public class MkGroupDetail
    {
        public long StudentId { get; set; }
        public string QuestionId { get; set; }
        public string Batch { get; set; }
        public decimal Score { get; set; }
    }

}
