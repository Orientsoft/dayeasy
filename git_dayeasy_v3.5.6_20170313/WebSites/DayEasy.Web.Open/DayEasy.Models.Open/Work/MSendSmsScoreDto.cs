using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DayEasy.Models.Open.Work
{
    public class MSendSmsScoreDto:DDto
    {
        public string Batch { get; set; }
        public string PaperId { get; set; }
        public string StudentIds { get; set; }

        private List<long> _list { get; set; } 
        public List<long> StudentIdList
        {
            get
            {
                if (_list == null) _list = new List<long>();
                if (_list.Any()) return _list;
                try
                {
                    if (!string.IsNullOrEmpty(StudentIds))
                        StudentIds.Split(',').Distinct().ToList()
                            .ForEach(s => _list.Add(Convert.ToInt64(s)));
                }
                catch { }
                return _list;
            }
        } 
    }
}
