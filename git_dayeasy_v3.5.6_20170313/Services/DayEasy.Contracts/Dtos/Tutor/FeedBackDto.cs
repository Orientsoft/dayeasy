using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contracts.Dtos.Tutor
{
    public class FeedBackDto : DDto
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string HeadPhoto { get; set; }
        public string Comment { get; set; }
        public DateTime Time { get; set; }
    }

    public class FeedBackPointDto : DDto
    {
        public string x { get; set; }
        public int y { get; set; }
    }
}
