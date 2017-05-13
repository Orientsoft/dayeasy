using System;
using System.Collections.Generic;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Group.Services.Helper;
using DayEasy.UnitTest.TestUtility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayEasy.Services.Helper;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class GroupContractTest : TestBase
    {
        private readonly IGroupContract _groupContract;

        public GroupContractTest()
        {
            _groupContract = Container.Resolve<IGroupContract>();
        }

        [TestMethod]
        public void GroupAvatar()
        {
            for (var i = 0; i < 10; i++)
            {
                var image = GroupAvatarHelper.Avatar();
                Console.WriteLine(image);
            }
        }

        [TestMethod]
        public void GroupCodeTest()
        {
            //            var manager = GroupCodeManager.Instance(_groupContract);
            //            for (int i = 0; i < 100; i++)
            //            {
            //                var code = manager.GroupCode(GroupType.Class);
            //                Console.WriteLine(code);
            //            }
        }

        [TestMethod]
        public void GroupsTest()
        {
            var groups = _groupContract.Groups(304533611023);
            Console.WriteLine(groups);
        }

        [TestMethod]
        public void CreateGroupTest()
        {

            var result = _groupContract.CreateGroup(new ColleagueGroupDto
            {
                Capacity = 200,
                Count = 0,
                CreationTime = DateTime.Now,
                ManagerId = 988769908616,
                GroupSummary = "测试一下",
                Name = "ybg的同事圈子",
                Type = (byte)GroupType.Colleague,
                AgencyId = "123456789",
                Stage = 1,
                SubjectId = 5
            }, (byte)UserRole.Teacher);
            Console.WriteLine(result);
        }
        [TestMethod]
        public void CreateGroupForManage()
        {
            const int appid = 1012;
            const int status = 0;
            var result = _groupContract.CreateGroupForManage(new ClassGroupDto
            {
                Capacity = 200,
                Count = 0,
                CreationTime = DateTime.Now,
                ManagerId = 988769908616,
                GroupSummary = "测试一下",
                Name = "大家一起来玩吧",
                Type = (byte)GroupType.Class,
                AgencyId = "cjiop76508d34f029c9fc4df03b80302",
                Stage = 1,
                GradeYear = 2016
            }, status, appid);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void LoadByCodeTest()
        {
            var result = _groupContract.LoadByCode("GP94597");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ApplyGroupTest()
        {
            var result = _groupContract.ApplyGroup("89392b4f43784d4ca159a293542c3589", 322936262622,
                "我还是让蒋公雕窝，加一个呗，嗯嗯嗯嗯。。");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void VerifyTest()
        {
            var result = _groupContract.Verify("39c67655944e46d78509b182fa8e55f6", CheckStatus.Normal, "已经加了你了");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void SearchGroupTest()
        {
            var result = _groupContract.SearchGroups(new SearchGroupDto()
            {
                AgencyId = "9e04bfc885504baea722f6a283c3e599",
                Keyword = "",
                Types = new List<int> { (byte)GroupType.Class },
                GradeYear = 2013
            });
            Console.WriteLine(result);
        }

        [TestMethod]
        public void GroupMembersTest()
        {
            var result = _groupContract.GroupMembers("89392b4f43784d4ca159a293542c3589");
            Console.WriteLine(result);
        }

        [TestMethod]
        public void ColleagueClassesTest()
        {
            const string id = "95c18d72c8c14b7fa89e39e8e84981aa";
            //var result = _groupContract.ColleagueClasses(id);
            //WriteJson(result);

            var dict = _groupContract.ColleagueClassDict(id);
            WriteJson(dict);
        }

        [TestMethod]
        public void LoadByIdTest()
        {
            var result = _groupContract.LoadById("2d79d3d8f05941579f9a48f9d6532f5a");
        }
        [TestMethod]
        public void BatchCreateGroups()
        {

            BatchCreateGroupsDto dto = new BatchCreateGroupsDto();
            dto.ClassGroups.Add(new ClassGroupDto { Name = "初caijie22019级1班", GradeYear = 2016 });
            dto.ClassGroups.Add(new ClassGroupDto { Name = "初caijie22019级2班", GradeYear = 2016 });
            dto.ColleagueGroups.Add(new ColleagueGroupDto { Name = "初caijie22019级英语组", SubjectId = 3 });

            WriteJson(_groupContract.BatchCreateGroups(dto, "0555a5fa82034968a484288d7a97d4d8"));
        }
        [TestMethod]
        public void BatchImportTeacher()
        {
            long[] ids = { 988769908616, 757467215117, 694350989072, 692516231514 };
            var result = _groupContract.BatchImportTeacher(ids, "c94b92df614b4bcfb660859780eae12e");
            WriteJson(result);
        }
        [TestMethod]
        public void GetGroupRepeatMsg()
        {
            BatchCreateGroupsDto dto = new BatchCreateGroupsDto();
            dto.ClassGroups = new List<ClassGroupDto> { new ClassGroupDto { Name = "初caijietes2019级1班" }, new ClassGroupDto { Name = "初caijietes2019级2班" } };
            dto.ColleagueGroups = new List<ColleagueGroupDto> { new ColleagueGroupDto { Name = "初caijietes2019级英语组" } };
            WriteJson(_groupContract.GetGroupRepeatMsg(dto, "cjiop76508d34f029c9fc4df03b80302"));

        }
        /// <summary>
        /// 批量导入学生测试
        /// </summary>
        [TestMethod]
        public void BatchImportStudentTest()
        {
            const string groupid = "1ad8a3b44c0242e2a217e1699deb8444";
            string[] students = { "小明1111死死死死死死死死死死死死死死死死死死死死死死死死死死死嗖嗖嗖死死死死死死", "张三1111死死死死死死死死死死死死死死死死死死死死死死死死死死死嗖嗖嗖死死死死死死" };
            const string agencyId = "cjiop76508d34f029c9fc4df03b80302";
            const int stage = 0;
            var result = _groupContract.BatchImportStudent(students, groupid, agencyId, stage);
            WriteJson(result);

        }
    }
}
