
using System;
using System.IO;
using System.Linq;
using DayEasy.Contracts.Models;
using DayEasy.Core.Dependency;
using DayEasy.Services;

namespace DayEasy.MigrateTools.Migrate
{
    public class PaperMigrate : MigrateBase
    {
        private readonly IDayEasyRepository<TP_Paper> _paperRepository;
        private readonly IDayEasyRepository<TP_PaperAnswer> _paperAnswerRepository;

        public PaperMigrate()
        {
            _paperAnswerRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_PaperAnswer>>();
            _paperRepository = CurrentIocManager.Resolve<IDayEasyRepository<TP_Paper>>();
        }

        public void ChangePaperAnswer()
        {
            var paperIds = "papers.txt".ReadConfig();
            foreach (var paperId in paperIds)
            {
                Console.WriteLine("正在同步：[{0}]", paperId);
                var id = paperId;
                var paper = _paperRepository.Load(id);
                if (paper == null)
                    continue;
                var answers = _paperAnswerRepository.Where(p => p.PaperId == id).ToList();
                if (answers.Any())
                {
                    //                    PaperTask.Instance.EditMyselfAnswerAsync(answers, paper.AddedBy).Wait();
                    Console.WriteLine("同步完成！");
                }
            }
        }
    }
}
