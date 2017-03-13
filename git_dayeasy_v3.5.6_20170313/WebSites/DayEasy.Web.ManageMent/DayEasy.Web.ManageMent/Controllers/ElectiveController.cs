using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using DayEasy.Contracts;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management;
using DayEasy.Contracts.Models;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Web.ManageMent.Filters;

namespace DayEasy.Web.ManageMent.Controllers
{
    [AdminRoles(AdminRole.ElectiveManager)]
    [RoutePrefix("elective")]
    public class ElectiveController : AdminController
    {
        public ElectiveController(IUserContract userContract, IManagementContract managementContract)
            : base(userContract, managementContract)
        {
        }

        /// <summary> 列表 </summary>
        [Route("list")]
        public ActionResult Index()
        {
            string key = RequestHelper.GetQueryString("txtKey"),
                agencyId = RequestHelper.GetQueryString("ddlAgency");

            System.Linq.Expressions.Expression<Func<TS_ElectiveCourse, bool>> condition =
                c => c.AgencyId == agencyId;
            if (key.IsNotNullOrEmpty())
                condition = condition.And(c => c.CourseName.Contains(key) || c.TeacherName.Contains(key));
            var courses = _facade.GetEntities(condition).ToList();
            ViewData["agencys"] = new TU_AgencyFacade()
                .GetEntities(t => t.Status == (byte)AgencyStatus.Normal)
                .OrderByDescending(t => t.ApplyAt)
                .ToList()
                .ToSlectListItemList(a => a.AgencyName, a => a.AgencyID, true, agencyId);

            return System.Web.UI.WebControls.View(courses);
        }

        /// <summary> 导入课程 </summary>
        public ActionResult ImportCourse()
        {
            var file = Request.Files["file"];
            if (file == null || file.ContentLength == 0)
                return System.Web.Helpers.Json(new JsonResultBase(false, "请选择文件！"), JsonRequestBehavior.AllowGet);

            var agencyId = RequestHelper.GetFormString("agencies");

            if (agencyId.IsNullOrEmpty())
                return System.Web.Helpers.Json(new JsonResultBase(false, "请选择机构！"), JsonRequestBehavior.AllowGet);

            var ds = ExcelHelper.Read(file.InputStream);
            if (ds == null || ds.Tables.Count < 1)
                return System.Web.Helpers.Json(new JsonResultBase(false, "文件解析失败！"), JsonRequestBehavior.AllowGet);

            if (ds.Tables[0].Rows[0].ItemArray.Length < 5)
                return System.Web.Helpers.Json(new JsonResultBase(false, "文件内容不符和条件！"), JsonRequestBehavior.AllowGet);

            var time = DateTime.Now;
            var userId = CurrentUser.UserId;

            var list = new List<TS_ElectiveCourse>();
            var dt = ds.Tables[0];
            foreach (DataRow dr in dt.Rows)
            {
                string dr0 = dr[0].ToString(),
                    dr1 = dr[1].ToString(),
                    dr2 = dr[2].ToString(),
                    dr3 = dr[3].ToString(),
                    dr4 = dr[4].ToString(),
                    dr5 = dr[5].ToString(),
                    dr6 = dr[6].ToString(),
                    dr7 = dr[7].ToString();
                if (dr0.IsNullOrEmpty() || dr1.IsNullOrEmpty() || dr2.IsNullOrEmpty() ||
                    dr5.IsNullOrEmpty() || dr6.IsNullOrEmpty() || dr7.IsNullOrEmpty())
                    continue;

                var item = new TS_ElectiveCourse
                {
                    CourseId = IdHelper.Instance.IntId,
                    AgencyId = agencyId,
                    Stage = 0,
                    Grade = 0,
                    Status = 0,
                    AddedBy = userId,
                    AddedAt = time,
                    CourseName = dr0,
                    TeacherName = dr1,
                    Address = dr2,
                    ClassCapacity = 0,
                    TotalCapacity = 0,
                    Coverage = dr5,
                    StartTime = dr6,
                    Category = dr7
                };
                if (dr3 != string.Empty)
                {
                    int classCapacity;
                    if (int.TryParse(dr3, out classCapacity))
                        item.ClassCapacity = classCapacity;
                }
                if (dr4 != string.Empty)
                {
                    int totalCapacity;
                    if (int.TryParse(dr4, out totalCapacity))
                        item.TotalCapacity = totalCapacity;
                }
                list.Add(item);
            }

            if (!list.Any())
                return System.Web.Helpers.Json(new JsonResultBase(false, "没有解析到数据！"), JsonRequestBehavior.AllowGet);

            try
            {
                var result = _facade.AddEntities(list.ToArray());
                return System.Web.Helpers.Json(
                    result > 0
                        ? new JsonResultBase(true, "导入成功！")
                        : new JsonResultBase(false, "导入失败！")
                    , JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return System.Web.Helpers.Json(new JsonResultBase(false, "写入数据失败！"), JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary> 编辑课程 </summary>
        /// <returns></returns>
        public ActionResult UpdateCourse()
        {
            int id = RequestHelper.GetFormInt32("txtId", 0),
                count = RequestHelper.GetFormInt32("txtCount", 0),
                total = RequestHelper.GetFormInt32("txtTotal", 0);

            string name = RequestHelper.GetFormString("txtName"),
                teacherName = RequestHelper.GetFormString("txtTeacherName"),
                address = RequestHelper.GetFormString("txtAddress"),
                time = RequestHelper.GetFormString("txtTime"),
                coverage = RequestHelper.GetFormString("txtCoverage");

            if (id < 1000)
                return System.Web.Helpers.Json(new JsonResultBase(false, "课程ID错误，请刷新重试"), JsonRequestBehavior.AllowGet);
            if (name.IsNullOrEmpty())
                return System.Web.Helpers.Json(new JsonResultBase(false, "课程名称不能为空"), JsonRequestBehavior.AllowGet);
            if (teacherName.IsNullOrEmpty())
                return System.Web.Helpers.Json(new JsonResultBase(false, "上课老师不能为空"), JsonRequestBehavior.AllowGet);
            if (address.IsNullOrEmpty())
                return System.Web.Helpers.Json(new JsonResultBase(false, "上课地址不能为空"), JsonRequestBehavior.AllowGet);

            var item = _facade.GetEntityByWhereLambda(i => i.CourseId == id);
            if (item == null)
                return System.Web.Helpers.Json(new JsonResultBase(false, "没有查询到课程数据"), JsonRequestBehavior.AllowGet);
            item.CourseName = name;
            item.TeacherName = teacherName;
            item.Address = address;
            item.StartTime = time;
            item.Coverage = coverage;
            item.ClassCapacity = count;
            item.TotalCapacity = total;

            return System.Web.Helpers.Json(
                _facade.UpdateEntity(item) != null
                    ? new JsonResultBase(true, "修改成功")
                    : new JsonResultBase(false, "修改失败")
                , JsonRequestBehavior.AllowGet);
        }

        /// <summary> 导出课程 </summary>
        public void ExportCourse()
        {
            var agencyId = RequestHelper.GetQueryString("agencyId");

            if (agencyId.IsNullOrEmpty())
            {
                Response.Write("请选择机构！");
                Response.End();
                return;
            }

            var electiveFacade = new TS_ElectiveCourseFacade();
            var courseFacade = new TS_StudentCourseFacade();
            var classFacade = new TU_ClassFacade();
            var userFacade = new TU_UserFacade();

            var electives = electiveFacade.GetEntities(e => e.Status == 0 && e.AgencyId == agencyId);
            var courseList = courseFacade.GetEntities(c => c.Status == 0)
                .Join(userFacade.GetEntities(u => true), s => s.StudentId, d => d.UserID, (s, d) => new { s, d })
                .Join(classFacade.GetEntities(c => true), s => s.s.ClassId, d => d.ClassID, (s, d) => new
                {
                    id = s.s.CourseId,
                    classGrade = d.GraduateYear,
                    className = d.ClassName,
                    classSort = d.Sort,
                    studentName = s.d.TrueName,
                });
            var eles = electives.GroupJoin(courseList, s => s.CourseId, d => d.id, (s, d) => new
            {
                d,
                electiveName = s.CourseName,
                teachername = s.TeacherName,
                address = s.Address,
                starttime = s.StartTime
            });

            var list = new List<VsElectiveCourseExport>();
            eles.ForEach(item => item.d.ForEach(d => list.Add(new VsElectiveCourseExport
            {
                Sort = d.classSort,
                ClassGrade = d.classGrade,
                ClassName = d.className,
                StudentName = d.studentName,
                ElectiveName = item.electiveName,
                TeacherName = item.teachername,
                Address = item.address,
                StartTime = item.starttime
            })));

            if (!list.Any())
            {
                Response.Write("没有可导出数据！");
                Response.End();
                return;
            }

            var ds = new DataSet();
            var dt = new DataTable("选修课报名统计");

            dt.Columns.Add("ClassGrade", typeof(string));
            dt.Columns.Add("ClassName", typeof(string));
            dt.Columns.Add("StudentName", typeof(string));
            dt.Columns.Add("ElectiveName", typeof(string));
            dt.Columns.Add("TeacherName", typeof(string));
            dt.Columns.Add("Address", typeof(string));
            dt.Columns.Add("StartTime", typeof(string));
            dt.Rows.Add(new object[] { "年级", "班级", "姓名", "选修课名称", "上课老师", "上课地址", "上课时间" });

            list.OrderBy(i => i.Sort).ForEach(i =>
                dt.Rows.Add(new object[] { i.ClassGrade, i.ClassName, i.StudentName, i.ElectiveName, i.TeacherName, i.Address, i.StartTime }));

            ds.Tables.Add(dt);
            ExcelHelper.Export(ds, "{0}选修课报名统计.xls".FormatWith(DateTime.Now.ToString("yyyy-M-d")));
        }

        /// <summary> 删除课程 </summary>
        public ActionResult Delete(int id)
        {
            if (id < 1000)
                return System.Web.Helpers.Json(new JsonResultBase(false, "参数错误！"), JsonRequestBehavior.AllowGet);

            _courseFacade.DeleteByLambda(t => t.CourseId == id);
            var result = _facade.DeleteByLambda(t => t.CourseId == id);
            return System.Web.Helpers.Json(
                result > 0
                    ? new JsonResultBase(true, "删除成功！")
                    : new JsonResultBase(false, "删除失败！")
                , JsonRequestBehavior.AllowGet);
        }

        /// <summary> 关闭课程 </summary>
        public ActionResult Close(int id)
        {
            var item = _facade.GetEntityByWhereLambda(c => c.CourseId == id);
            if (item == null)
                return System.Web.Helpers.Json(new JsonResultBase(false, "没有找到课程资料！"), JsonRequestBehavior.AllowGet);

            //_courseFacade.UpdateEntityBySql(
            //    new Dictionary<string, object> { { "Status", 4 } }, "CourseId=" + id);

            item.Status = 4;
            var result = _facade.UpdateEntity(item, "Status");
            return System.Web.Helpers.Json(
                result != null
                    ? new JsonResultBase(true, "关闭成功！")
                    : new JsonResultBase(false, "关闭失败！")
                , JsonRequestBehavior.AllowGet);
        }


    }
}