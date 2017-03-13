using DayEasy.ThirdPlatform;
using DayEasy.Utility.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Xml.Serialization;

namespace FrameworkTest
{
    [TestClass]
    public class HelperTest
    {
        [Serializable]
        [XmlRoot("xml")]
        public class WeixinDto
        {
            [XmlElement]
            public string return_code { get; set; }
            [XmlElement]
            public string return_msg { get; set; }
            [XmlElement]
            public string result_code { get; set; }
            [XmlElement]
            public string trade_type { get; set; }
            [XmlElement]
            public string prepay_id { get; set; }
        }


        [TestMethod]
        public void XmlHelperTest()
        {
            const string msg = "<xml><return_code><![CDATA[FAIL]]></return_code><return_msg><![CDATA[appid不存在]]></return_msg></xml>";
            var dto = XmlHelper.XmlDeserialize<WeixinDto>(msg, Encoding.Default);
            Console.Write(dto);
        }
        [TestMethod]
        public void QQLogin()
        {
          var plat=  PlatformFactory.GetInstance(0);
          var data=  plat.User("A312C2C225D1D985C255FBCD4CCD0DAC");
            Console.Write(data);
        }
    }
}
