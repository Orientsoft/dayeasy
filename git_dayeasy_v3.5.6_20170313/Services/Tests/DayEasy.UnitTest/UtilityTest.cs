using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using DayEasy.AutoMapper;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Models;
using DayEasy.Core.Config;
using DayEasy.Core.Dependency;
using DayEasy.MemoryDb.Redis;
using DayEasy.Services;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility;
using DayEasy.Utility.Config;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.License;
using DayEasy.Utility.Logging;
using DayEasy.Utility.Timing;
using DayEasy.Utility.UseTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest
{
    [TestClass]
    public class UtilityTest : TestBase
    {
        private readonly ILogger _logger = LogManager.Logger<UtilityTest>();

        [TestMethod]
        public void EmailTest()
        {
            var user = new UserDto
            {
                Id = 123456,
                ExpireTime = Clock.Now,
                Role = 8
            };
            Console.WriteLine(user);
        }

        [TestMethod]
        public void ImageTest()
        {
            var image = Image.FromFile("1.gif");
            var bmp = ImageHelper.ResizeImg((Bitmap)image, 200, 200);
            bmp.Save("3.gif");
        }

        private class MyClass
        {
            public DateTime CreateDate { get; set; }
            public string UserNAME { get; set; }

            public MyClass(string name)
            {
                UserNAME = name;
                CreateDate = Clock.Now;
            }
        }

        [TestMethod]
        public void JsonTest()
        {
            var json = new
            {
                CreateDate = Clock.Now,
                UserNAME = "shoy"
            };

            var jsonStr = JsonHelper.ToJson(json, NamingType.CamelCase, true, false, "createDate");
            Console.WriteLine(jsonStr);

            const int interaction = 1000 * 1;
            var timer01 = CodeTimer.Time("JavaScriptSerializer", () =>
            {
                var data = json.ToJson();
            }, interaction);
            var timer02 = CodeTimer.Time("Json.Net", () =>
            {
                var data = JsonHelper.ToJson(json, props: "date");
            }, interaction);
            Console.WriteLine(timer01);
            Console.WriteLine(timer02);
            var str = JsonHelper.ToJson(json, NamingType.CamelCase);
            Console.WriteLine(str);
            var other = new
            {
                CreateDate = DateTime.MinValue,
                UserNAME = string.Empty
            };
            other = JsonHelper.Json(str, other, NamingType.CamelCase);
            Console.WriteLine(other.CreateDate);
            Console.WriteLine(other.UserNAME);

            var list = new List<MyClass>
            {
                new MyClass("shoy"),
                new MyClass("love"),
                new MyClass("programing")
            };
            var listJson = JsonHelper.ToJson(list, NamingType.UrlCase);
            Console.WriteLine(listJson);
            var list1 = JsonHelper.Json<List<MyClass>>(listJson, NamingType.UrlCase);
            Console.WriteLine(list1.ToJson());
        }

        [TestMethod]
        public void LamdaTest()
        {
            var user = new UserDto
            {
                Id = 2512,
                SubjectId = 5
            };
            user.Email = "ddd@qq.com";
            Expression<Func<UserDto, UserDto>> expression = u => user;
            Console.WriteLine(JsonHelper.ToJson(expression.ToDictionary(), NamingType.Normal, true));
        }

        [TestMethod]
        public void IocTest()
        {
            var userRepository = CurrentIocManager.Resolve<IDayEasyRepository<TU_User, long>>();
            var user = userRepository.Load(304533611023).MapTo<UserDto>();
            Console.WriteLine(user);

            var groupRepository = CurrentIocManager.Resolve<IVersion3Repository<TG_Group>>();
            var group = groupRepository.Load("0315a64000794403be1273b650096d70");
            Console.WriteLine(group.MapTo<GroupDto>());
        }

        [TestMethod]
        public void RedisTest()
        {
            //            var site = Consts.Config.AccountSite;
            //            Console.WriteLine(site.Substring(site.IndexOf("//", StringComparison.Ordinal)));
            //            RedisUtils.DeleteAll("dayeasy___question_v3_*");
            for (var i = 0; i < 11; i++)
            {
                Console.WriteLine(IdHelper.Instance.Guid32);
            }
        }

        public class ShoyTest
        {
            public int Id { get; set; }
            public Dictionary<int, string> Dict { get; set; }
        }

        [TestMethod]
        public void JsonHelperTest()
        {
            var dict = new Dictionary<int, string>();
            dict.Add(1025, "test");
            var model = new ShoyTest
            {
                Id = 123,
                Dict = dict
            };
            var json = JsonHelper.ToJson(model, NamingType.CamelCase, true);
            Console.WriteLine(json);

            model = JsonHelper.Json<ShoyTest>(json, NamingType.CamelCase);
            Console.WriteLine(JsonHelper.ToJson(model, NamingType.CamelCase, true));
        }

        [TestMethod]
        public void SetQueryTest()
        {
            var helper =
                new HttpHelper(
                    "http://file.dayeasy.net/upload/paper/marking/201511/d67899552d0249ddbcd8b6bbecc7c96f/85c7550069.jpg");
            var img = ImageHelper.MakeImage(helper.GetStream(),
                (int)28.5, (int)1911, (int)736.5, (int)275);
            Console.WriteLine(img);
        }

        [TestMethod]
        public void MosaicImageTest()
        {
            var url = @"http://file.dayez.net/upload/paper/marking/201601/71c838e658c945179dbc634a67ff8de8/%E5%86%85%E6%B5%8B%E7%89%A9%E7%90%86%E8%AF%95%E5%8D%B7A00020A.jpg";
            var helper = new HttpHelper(url);
            var str = ImageHelper.MosaicImage(helper.GetStream(), 0, 0, 0, 125);
            Console.Write(str);
        }

        [TestMethod]
        public void ArrayTest()
        {
            //测试数组交集、差集、并集
            List<int>
                a = new List<int> { 1, 2, 3, 4 },
                b = new List<int> { 3, 2, 1, 5 };
            Console.WriteLine("交集：" + (a.Intersect(b)).ToJson());
            Console.WriteLine("差集：" + (a.Except(b)).ToJson());
            Console.WriteLine("并集：" + (a.Union(b)).ToJson());
        }

        [TestMethod]
        public void Array2dTest()
        {
            //var array2d = new string[10, 10];
            string[][] array2d = new string[3][];
            array2d[0] = new[] { "1", "1.1" };
            array2d[1] = new[] { "2", "2.2" };
            array2d[2] = new[] { "3", "3.3" };

            foreach (var array in array2d)
            {
                Console.WriteLine(array.ToJson());
            }
        }

        [TestMethod]
        public void GuidTest()
        {
            Console.WriteLine(IdHelper.Instance.Guid32);
            Console.WriteLine(IdHelper.Instance.Guid32);
        }

        [TestMethod]
        public void CodeTest()
        {
            LicenseManager.SetGenerateRole(LicenseType.DCode, DCode.Code);
            var list = new List<string>();
            var helper = LicenseManager.Instance(LicenseType.DCode);
            var watch = new Stopwatch();
            for (int i = 0; i < 80000 - 29; i++)
            {
                watch.Start();
                var code = helper.Code();
                list.Add(code);
                if (i % 1000 == 0)
                {
                    watch.Stop();
                    Console.WriteLine("{0}:{1}", i, watch.ElapsedMilliseconds);
                    watch.Restart();
                }
            }
            Console.WriteLine(list.GroupBy(t => t).Where(t => t.Count() > 1).Select(t => new
            {
                code = t.Key,
                coune = t.Count()
            }).ToJson());
        }

        [TestMethod]
        public void OptionTest()
        {
            var root = Path.IsPathRooted("D:/DayEz/config/v3/index.html");
            Console.WriteLine(root);

            var answera = "ABD".Select(answer => (int)(answer) - 65).ToList();
            Console.WriteLine(answera.ToJson());
        }

        [TestMethod]
        public void ConfigTest()
        {
            var config = ConfigUtils<RecommendImageConfig>.Config;
            Assert.AreNotEqual(config, null);
            Console.Write(config.ToJson());
        }

        [TestMethod]
        public void ImageResizeTest()
        {
            Console.WriteLine(4|16|128 & (2|4));
//            var picture = "http://file.dayez.net/upload/paper/marking/201606/1666f0bab7484a408257ea2abe477ca7/%E8%8B%B1%E8%AF%AD%E6%B5%8B%E8%AF%95%E8%AF%86%E5%88%AB00001.jpg".PaperImage(0, 150, 450, 200);
//            Console.WriteLine(picture);
//            var index = "初2016级10班".As<IRegex>().Match("(\\d+)?班", 1).To(0);
//            Console.WriteLine(index);
            //            var rect = ImageHelper.CalcResizeRect(315, 154, 150, -1);
            //            Console.WriteLine(new
            //            {
            //                x = rect.X,
            //                y = rect.Y,
            //                w = rect.Width,
            //                h = rect.Height
            //            }.ToJson());

//            RedisUtils.DeleteAll("dayeasy__*");
        }

        [TestMethod]
        public void MarkingImageTest()
        {
            var image = Image.FromFile("1.jpg");
            var regions = new List<RectangleF>
            {
                new RectangleF(37.5F, 224F, 209.5F, 47F),
                new RectangleF(250.5F, 228F, 230F, 40F),
                new RectangleF(480.5F, 233F, 230.5F, 40F),
                new RectangleF(37.5F, 270F, 209.5F, 45F),
                new RectangleF(248.5F, 274F, 230F, 40F)
            };
            var region = ImageCls.CombineRegion(regions, 20);
            Console.WriteLine(JsonHelper.ToJson(region, NamingType.CamelCase, true));
            using (var bitmap = ImageHelper.MakeImage((Bitmap) image, region))
            {
                bitmap.Save("2.jpg");
            }
        }
    }
}
