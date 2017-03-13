using DayEasy.AsyncMission.Models;
using DayEasy.Utility.Config;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DayEasy.AsyncMission.Jobs
{
    [Serializable]
    [XmlRoot("root")]
    [FileName("jobs.config")]
    public class JobsConfig : ConfigBase
    {
        [XmlAttribute("interval")]
        public double Interval { get; set; }

        [XmlArray("jobs")]
        [XmlArrayItem("job")]
        public List<JobConfig> Jobs { get; set; }
    }

    public class JobConfig
    {
        [XmlAttribute("type")]
        public MissionType Type { get; set; }

        [XmlAttribute("interval")]
        public double Interval { get; set; }
    }
}
