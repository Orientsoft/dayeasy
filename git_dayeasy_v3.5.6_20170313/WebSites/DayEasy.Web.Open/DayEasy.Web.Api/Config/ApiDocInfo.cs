using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DayEasy.Utility.Config;

namespace DayEasy.Web.Api.Config
{
    /// <summary> API文档信息类 </summary>
    [Serializable]
    [XmlRoot("root")]
    [FileName("api_doc.config")]
    public class ApiDocInfo : ConfigBase
    {
        /// <summary>
        /// 默认url
        /// </summary>
        [XmlElement("def-url")]
        public string Url { get; set; }

        /// <summary>
        /// 默认数据类型
        /// </summary>
        [XmlElement("def-type")]
        public string DataType { get; set; }

        /// <summary>
        /// 默认的请求方法
        /// </summary>
        [XmlElement("def-method")]
        public string Method { get; set; }

        /// <summary>
        /// API接口信息
        /// </summary>
        [XmlElement("category")]
        public List<ApiCategoryInfo> Cates { get; set; }

        /// <summary>
        /// 系统状态码
        /// </summary>
        [XmlArray("codes")]
        [XmlArrayItem("item")]
        public List<StatusCode> StatusCodes { get; set; }

        public ApiDocInfo()
        {
            Cates = new List<ApiCategoryInfo>();
            StatusCodes = new List<StatusCode>();
        }
    }

    /// <summary>
    /// 接口方法分类
    /// </summary>
    [Serializable]
    public class ApiCategoryInfo
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        /// <summary>
        /// 分类名称
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// 接口方法列表
        /// </summary>
        [XmlElement("method")]
        public List<ApiMethodInfo> Methods { get; set; }

        public ApiCategoryInfo()
        {
            Methods = new List<ApiMethodInfo>();
        }
    }

    /// <summary>
    /// 接口方法类
    /// </summary>
    [Serializable]
    public class ApiMethodInfo
    {
        [XmlAttribute("id")]
        public int Id { get; set; }
        /// <summary>
        /// 接口名称
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// 接口名称
        /// </summary>
        [XmlAttribute("method")]
        public string Method { get; set; }
        
        /// <summary>
        /// 接口链接
        /// </summary>
        [XmlAttribute("url")]
        public string Url { get; set; }
        /// <summary>
        /// 支持的请求方式
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }
        /// <summary>
        /// 支持的数据类型
        /// </summary>
        [XmlAttribute("data-type")]
        public string DataType { get; set; }

        /// <summary>
        /// 应用场景
        /// </summary>
        [XmlElement("scenarios")]
        public string Scenarios { get; set; }
        /// <summary>
        /// 接口描述
        /// </summary>
        [XmlElement("desc")]
        public string Description { get; set; }
        
        
        /// <summary>
        /// 请求参数列表
        /// </summary>
        [XmlArray("response")]
        [XmlArrayItem("item")]
        public List<ApiParameter> Response { get; set; }

        /// <summary>
        /// 返回参数列表
        /// </summary>
        [XmlArray("request")]
        [XmlArrayItem("item")]
        public List<ApiParameter> Request { get; set; }

        public ApiMethodInfo()
        {
            Response = new List<ApiParameter>();
            Request=new List<ApiParameter>();
        }
    }

    /// <summary>
    /// 参数类
    /// </summary>
    [Serializable]
    public class ApiParameter
    {
        /// <summary>
        /// 参数名称
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }
        /// <summary>
        /// 参数类型
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }
        /// <summary>
        /// 是否必须
        /// </summary>
        [XmlAttribute("require")]
        public bool IsRequire { get; set; }
        /// <summary>
        /// 参数描述
        /// </summary>
        [XmlText]
        public string Description { get; set; }
    }

    /// <summary>
    /// 状态码
    /// </summary>
    [Serializable]
    public class StatusCode
    {
        /// <summary>
        /// 状态码
        /// </summary>
        [XmlAttribute("code")]
        public int Code { get; set; }
        /// <summary>
        /// 状态码类型
        /// </summary>
        [XmlAttribute("type")]
        public int Type { get; set; }
        /// <summary>
        /// 状态码信息
        /// </summary>
        [XmlAttribute("msg")]
        public string Msg { get; set; }
        /// <summary>
        /// 状态码描述
        /// </summary>
        [XmlText]
        public string Description { get; set; }
    }
}