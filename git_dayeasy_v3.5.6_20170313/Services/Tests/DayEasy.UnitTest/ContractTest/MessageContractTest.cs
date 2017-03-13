using System;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Message;
using DayEasy.Contracts.Enum;
using DayEasy.Core.Dependency;
using DayEasy.Core.Domain;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest.ContractTest
{
    [TestClass]
    public class MessageContractTest : TestBase
    {
        private readonly IMessageContract _messageContract;

        public MessageContractTest()
        {
            _messageContract = Container.Resolve<IMessageContract>();
        }

        [TestMethod]
        public void GetDynamicsTest()
        {
            var messages = _messageContract.GetDynamics(new DynamicSearchDto
            {
                GroupId = "aa359e3f5c5242c495bc86c98a91b1b6",
                Role = UserRole.Teacher,
                UserId = 757275382562,
                Page = 0,
                Size = 15
            });
            Console.WriteLine(messages);
        }

        [TestMethod]
        public void SendNoticeTest()
        {
            var result = _messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Notice,
                GroupId = "c702351ad6634228978bcba13ef8056f",
                Title = "",
                Message = "明天大家聚个餐呗！~",
                ReceivRole = (UserRole.Student | UserRole.Teacher),
                UserId = 304533611023
            });
            Console.WriteLine(result);
        }

        [TestMethod]
        public void SendHomeWorkDynamicTest()
        {
            var result = _messageContract.SendDynamic(new DynamicSendDto
            {
                DynamicType = GroupDynamicType.Homework,
                GroupId = "83ed7d31374040d2b2490494a26bd3be",
                ContentType = (byte) ContentType.Paper,
                ContentId = "f455ca959bd942b0b7ba158f4317f1eb",
                Message = "大家转推给本班学生吧~",
                ReceivRole = UserRole.Teacher,
                UserId = 304533611023
            });
            Console.WriteLine(result);
        }

        [TestMethod]
        public void UserMessageTest()
        {
            var json = _messageContract.UserMessages(304533611023, (int) MessageType.System);
            Console.WriteLine(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }
    }
}
