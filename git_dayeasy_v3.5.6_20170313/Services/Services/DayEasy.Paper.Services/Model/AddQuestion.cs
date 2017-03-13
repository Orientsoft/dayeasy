using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Paper.Services.Model
{
    public class UploadResult
    {
        public int state { get; set; }
        public List<string> urls { get; set; }
    }
    public enum UploadType
    {
        Image = 1,
        Video = 2,
        Other = 3
    }
}
