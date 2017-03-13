using System;
using Autofac;
using DayEasy.Core;
using DayEasy.Utility.Extend;
using DayEasy.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest
{
    [TestClass]
    public class EntityFrameworkTest : IDependency
    {
        //        private static EntityFrameworkTest _test;
        //        public IUserContract UserService { get; set; }
        //
        //        public ISubjectContract SubjectService { get; set; }
        //
        //        public IQuestionContract QuestionService { get; set; }

        static EntityFrameworkTest()
        {
            DBootstrap.Start(autoBuild: true);
            //            using (var scope = Startup.Container.BeginLifetimeScope())
            //            {
            //                _test = scope.Resolve<EntityFrameworkTest>();
            //            }
            //            _test = Startup.Container.Resolve<EntityFrameworkTest>();
        }

        [TestMethod]
        public void TestMethod1()
        {
            using (var scope = DBootstrap.Container.BeginLifetimeScope())
            {
                var userService = scope.Resolve<IUserContract>();
                var user = userService.Login("shoy");
                if (user != null)
                {
                    //分布式事务
                    //                DTransaction.Use(() =>
                    //                {
                    //                    user.Account = "shoy";
                    //                    _test.UserService.Update(user);
                    //                    var sub = _test.SubjectService.Load(user.SubjectId);
                    //                    sub.Name = "数学";
                    //                    _test.SubjectService.Update(sub);
                    //                }, IsolationLevel.ReadCommitted, TimeSpan.FromMinutes(5));
                    var subjectService = scope.Resolve<ISubjectContract>();
                    var sub = subjectService.SubjectRepository.Load(user.SubjectId);
                    Console.WriteLine(user.Name);
                    Console.WriteLine(sub.Name);
                }
            }
        }

        /// <summary> 生成数据库 </summary>
        [TestMethod]
        public void GenerateDb()
        {
            //            var init = new DropCreateDatabaseAlways<QuestionDbContext>();
            //            using (var db = new QuestionDbContext())
            //            {
            //                init.InitializeDatabase(db);
            //            }
        }

        [TestMethod]
        public void QuestionTest()
        {
            using (var scope = DBootstrap.Container.BeginLifetimeScope())
            {
                var questionService = scope.Resolve<IQuestionContract>();
                //                var id = test.QuestionService.Add(new DQuestion
                //                {
                //                    Id = IdHelper.Instance.Guid32,
                //                    Type = 1,
                //                    SubjectId = 1001,
                //                    Stage = 2,
                //                    Body = "测试题目，Oh yes!",
                //                    CreatorId = 1,
                //                    CreationTime = DateTime.Now
                //                });
                //PG
                var item = questionService.QuestionRepository.Load("2edd7659920c4ae18f28dc72ed5f0657");

                //ShoyWeb
                var userService = scope.Resolve<IUserContract>();
                var user = userService.UserRepository.Load(item.CreatorId);
                //                var user = userService.Load(item.CreatorId);

                //ShoyWeb_SystemDB
                var subjectService = scope.Resolve<ISubjectContract>();
                var sub = subjectService.SubjectRepository.Load(user.SubjectId);

                subjectService.SubjectRepository.UnitOfWork.Transaction(() =>
                {

                });

                Console.WriteLine(new
                {
                    id = item.Id,
                    body = item.Body,
                    type = item.Type,
                    stage = item.Stage,
                    time = item.CreationTime,
                    name = user.Name,
                    subject = sub.Name
                }.ToJson());
            }
        }
    }
}
