using System.Collections.Generic;
using DayEasy.Core.Domain.Entities;

namespace DayEasy.Contract.Open.Dtos
{
    public class UploaderResultDto : DDto
    {
        public int state { get; set; }
        public List<string> urls { get; set; }
    }
}
