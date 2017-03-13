using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using DayEasy.Contracts.Dtos.Elective;
using DayEasy.Office;
using DayEasy.Utility.Extend;

namespace DayEasy.Application.Services.Helper
{
    public class ElectiveHelper
    {
        public static List<CourseItemDto> Import(string name, string fileData)
        {
            var list = new List<CourseItemDto>();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(fileData))
                return list;
            var file = fileData.Split(';');
            if (file.Length != 2)
                return list;
            //            var ext = Path.GetExtension(name);
            var arr = Convert.FromBase64String(file[1].Replace("base64,", string.Empty));
            using (var stream = new MemoryStream(arr))
            {
                var ds = ExcelHelper.Read(stream);
                if (ds == null || ds.Tables.Count == 0)
                    return list;
                var table = ds.Tables[0];
                var index = 1;
                foreach (DataRow row in table.Rows)
                {
                    if (row[0] == null || row[1] == null || row[2] == null || row[3] == null)
                        continue;
                    var capacity = row[3].CastTo(-1);
                    if (capacity <= 0)
                        continue;
                    var item = new CourseItemDto
                    {
                        Name = row[0].ToString(),
                        Teacher = row[1].ToString(),
                        Address = row[2].ToString(),
                        Capacity = capacity
                    };
                    if (string.IsNullOrWhiteSpace(item.Name)
                        || string.IsNullOrWhiteSpace(item.Teacher)
                        || string.IsNullOrWhiteSpace(item.Address))
                        continue;
                    item.Id = index++;
                    list.Add(item);
                }
            }
            return list;
        }

        public static void Export(string title, List<ElectiveDetailDto> details)
        {
            var ds = new DataSet();
            var dt = new DataTable("选课统计");
            dt.Columns.Add("name", typeof (string));
            dt.Columns.Add("teacher", typeof (string));
            dt.Columns.Add("address", typeof (string));
            dt.Columns.Add("capacity", typeof (string));
            dt.Columns.Add("current", typeof (string));
            dt.Columns.Add("student", typeof (string));
            dt.Columns.Add("group", typeof (string));
            dt.Rows.Add("课程名称", "任课老师", "上课地点", "人数限制", "报名人数", "学生名字", "所在班级");
            foreach (var detail in details)
            {
                if (detail.Students == null || !detail.Students.Any())
                {
                    dt.Rows.Add(detail.CourseName, detail.Teacher, detail.Address, detail.Capacity, detail.Current,
                        string.Empty, string.Empty);
                    continue;
                }
                foreach (var student in detail.Students)
                {
                    dt.Rows.Add(detail.CourseName, detail.Teacher, detail.Address, detail.Capacity, detail.Current,
                        student.Name, student.ClassNmae);
                }
            }
            ds.Tables.Add(dt);
            ExcelHelper.Export(ds, title + "-选课统计.xls");
        }

        public static void DownloadTemplete()
        {
            var ds = new DataSet();
            var dt = new DataTable("选课");
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("teacher", typeof(string));
            dt.Columns.Add("address", typeof(string));
            dt.Columns.Add("capacity", typeof(string));
            dt.Rows.Add("课程名称", "任课教师", "上课地点", "限报人数");
            ds.Tables.Add(dt);
            ExcelHelper.Export(ds, "选课模板.xls");
        }
    }
}
