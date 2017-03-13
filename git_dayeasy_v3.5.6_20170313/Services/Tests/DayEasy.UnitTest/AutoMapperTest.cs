using System;
using DayEasy.AutoMapper;
using DayEasy.AutoMapper.Attributes;
using DayEasy.UnitTest.TestUtility;
using DayEasy.Utility.Extend;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DayEasy.UnitTest
{
    [TestClass]
    public class AutoMapperTest : TestBase
    {
        [TestMethod]
        public void MapperTest()
        {
            var item = new MyClass1("shoy");
            var other = item.MapTo<MyClass2>();
            Console.Write(other.ToJson());
        }

        [TestMethod]
        public void FlagsTest()
        {
            var flag = AutoMapDirection.To.HasFlag(AutoMapDirection.From);
            Console.WriteLine(flag);
            flag = AutoMapDirection.From.HasFlag(AutoMapDirection.To);
            Console.WriteLine(flag);
        }

        [AutoMapFrom(typeof(MyClass1))]
        private class MyClass
        {
            public string Name { get; set; }

            [MapFrom("Name")]
            public string OtherName { get; set; }

            public MyClass(string name, string other)
            {
                Name = name;
                OtherName = other;
            }
        }

        [AutoMapTo(typeof(MyClass2))]
        private class MyClass1
        {
            [MapFrom("OtherName")]
            public string Name { get; set; }
            public MyClass1(string name)
            {
                Name = name;
            }
        }

        private class MyClass2
        {
            public string Name { get; set; }

            public string OtherName { get; set; }
            public MyClass2(string name)
            {
                Name = name;
            }
        }
    }
}
