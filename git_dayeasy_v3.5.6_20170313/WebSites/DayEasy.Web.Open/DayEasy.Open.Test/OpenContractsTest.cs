using Autofac;
using DayEasy.Contract.Open.Contracts;
using DayEasy.Contract.Open.Helper;
using DayEasy.Contracts;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Linq;
using DayEasy.Utility.Timing;

namespace DayEasy.Open.Test
{
    [TestClass]
    public class OpenContractsTest : OpenTestBase
    {
        private IOpenContract _openContract;
        private IExaminationContract _examinationContract;


        //public OpenContractsTest()
        //{
        //    _openContract = Container.Resolve<IOpenContract>();
        //    _examinationContract = Container.Resolve<IExaminationContract>();
        //}

        [TestMethod]
        public void JointPictureTest()
        {
            //Console.WriteLine();

            //var jointBatch = "2a1de2e2d2a840659deb01343799263b";
            //var pictures = new[] { "" };
            //HandinPicturesTask.Instance.InitJointPictures(jointBatch, pictures.ToList());
        }

        [TestMethod]
        public void EncodePwdTest()
        {
            var pwd = "pxHU6UQjaGE%7E%0A";
            var word = "123456";
            Console.WriteLine(pwd.DecodePwd());
            Console.WriteLine(word.EncodePwd());
        }

        [TestMethod]
        public void JointPrintListTest()
        {
            const string paperId = "46d1524807d54efaa11231de44c079ad";
            var result = _openContract.JointPrintList(paperId);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true, false, "markingDate"));
        }

        [TestMethod]
        public void JointAgenciesTest()
        {
            const string joint = "0d48c11c8f254dfcafba4cad6bdf3a02";
            var result = _openContract.JointAgencies(joint);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void JointPrintDetailsTest()
        {
            const string joint = "0d48c11c8f254dfcafba4cad6bdf3a02";
            var result = _openContract.JointPrintDetails(joint, 1);
            Console.WriteLine(JsonHelper.ToJson(result, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void AnswerPaperTest()
        {
            //4500cec8b0e34e5ebcff2a17ed42e976
            //950daaf69b044dd4be5244cfd94b9daf
            var sw = new Stopwatch();
            sw.Start();
            var json = _openContract.ErrorStatistics("f58b162915a9436a81636755e0f9828b");
            sw.Stop();
            Console.WriteLine("耗时(毫秒)：" + sw.ElapsedMilliseconds);
            Console.WriteLine();
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void SummaryTest()
        {
            var json = _examinationContract.Summary("f1da0df89bb543d3b70266ecd46881af", 322936262624);
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void VariantTest()
        {
            var json = _openContract.PaperVariant("f7317d3c209c46b29c0e0da1fb695615", "46d1524807d54efaa11231de44c079ad");
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void NewVariantTest()
        {
            var json = _openContract.NewVariant("c28b28e74b3244eab54b2e1900398b8f");
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void Md5Test()
        {
            const string word = "这里是评语哦!";
            Console.WriteLine(word.Md5().ToLower());
        }

        [TestMethod]
        public void ScoreStatisticsTest()
        {
            var json = _openContract.ScoreStatistics("0eb1baac701344a2bf6877031ae23df9", "f8156eb0f0f042d69a982d13b730dccd", 692589746681);
            Console.Write(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void SearchStudentTest()
        {
            var json = _openContract.SearchStudent("甘宝儿");
            Console.WriteLine(JsonHelper.ToJson(json, NamingType.CamelCase, true));
        }
    }
}
