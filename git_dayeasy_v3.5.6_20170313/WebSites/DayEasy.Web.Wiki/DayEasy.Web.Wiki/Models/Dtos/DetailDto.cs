using System;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Web.Wiki.Models.Dtos
{
    [Serializable]
    [AutoMap(typeof(WikiDetail))]
    public class DetailDto : DEntity<string>
    {
        public string Title { get; set; }
        public string Detail { get; set; }
        public int Sort { get; set; }
        public string Version { get; set; }
    }
}