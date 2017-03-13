using System;
using System.Collections.Generic;
using DayEasy.AutoMapper.Attributes;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Web.Wiki.Models.Dtos
{
    [Serializable]
    [AutoMap(typeof(WikiGroup))]
    public class GroupDto : DEntity<string>
    {
        [MapFrom("Group")]
        public string Name { get; set; }

        public string Code { get; set; }

        public string Logo { get; set; }

        public int Sort { get; set; }

        public List<string> Wikis { get; set; }

        public GroupDto()
        {
            Wikis = new List<string>();
        }
    }
}