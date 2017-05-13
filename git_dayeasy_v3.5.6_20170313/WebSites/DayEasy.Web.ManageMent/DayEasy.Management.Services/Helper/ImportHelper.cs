using DayEasy.Contracts.Dtos.Marking.Joint;
using DayEasy.Utility.Extend;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DayEasy.Management.Services.Helper
{
    public static class ImportHelper
    {
        public static List<JDataInputDto> ParseJData(this DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count == 0)
                return new List<JDataInputDto>();
            var dt = dataSet.Tables[0];
            var dtos = new List<JDataInputDto>();
            foreach (DataRow row in dt.Rows)
            {
                var cols = row.ItemArray;
                if (cols.Length < 3) continue;
                var name = (cols[0] ?? string.Empty).ToString();
                if (string.IsNullOrWhiteSpace(name) || name == "姓名")
                    continue;
                var groupCode = (cols[1] ?? string.Empty).ToString();
                if (string.IsNullOrWhiteSpace(groupCode) || !groupCode.As<IRegex>().IsMatch("GC\\d+"))
                    continue;
                dtos.Add(new JDataInputDto
                {
                    Student = name,
                    GroupCode = groupCode,
                    Scores = cols.Skip(2).Select(t => t.CastTo<decimal>()).ToList()
                });
            }
            return dtos;
        }
    }
}

