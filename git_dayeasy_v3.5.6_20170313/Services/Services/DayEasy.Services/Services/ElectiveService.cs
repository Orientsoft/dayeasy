using DayEasy.AutoMapper;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Elective;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.EntityFramework;
using DayEasy.Utility;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DayEasy.Services.Services
{
    public class ElectiveService : DayEasyService, IElectiveContract
    {
        public IDayEasyRepository<TS_ElectiveBatch> ElectiveBatchRepository { private get; set; }
        public IDayEasyRepository<TS_ElectiveCourse, int> ElectiveCourseRepository { private get; set; }
        public IDayEasyRepository<TS_ElectiveDetail> ElectiveDetailRepository { private get; set; }

        public IUserContract UserContract { private get; set; }
        public IGroupContract GroupContract { private get; set; }

        public ElectiveService(IDbContextProvider<DayEasyDbContext> context)
            : base(context)
        {
        }

        /// <summary> 创建选修课 </summary>
        public DResult Create(ElectiveInputDto dto)
        {
            if (dto == null || dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.AgencyId))
                return DResult.Error("参数错误！");
            if (string.IsNullOrWhiteSpace(dto.Title))
                return DResult.Error("请输入选修课标题！");
            if (dto.Courses == null || !dto.Courses.Any())
                return DResult.Error("必须包含至少一门选修课！");
            var helper = IdHelper.Instance;
            var model = new TS_ElectiveBatch
            {
                Id = helper.Guid32,
                Title = dto.Title,
                AgencyId = dto.AgencyId,
                CreatorId = dto.UserId,
                CreationTime = Clock.Now,
                Status = (byte)ElectiveStatus.Normal
            };
            var list = new List<TS_ElectiveCourse>();
            foreach (var course in dto.Courses)
            {
                var item = new TS_ElectiveCourse
                {
                    CourseName = course.Name,
                    TeacherName = course.Teacher,
                    Address = course.Address,
                    TotalCapacity = course.Capacity,
                    ClassCapacity = course.ClassCapacity,
                    Batch = model.Id,
                    AgencyId = dto.AgencyId,
                    AddedBy = dto.UserId,
                    AddedAt = Clock.Now,
                    Status = (byte)NormalStatus.Normal
                };
                if (course.ClassList != null && course.ClassList.Any(t => t != string.Empty))
                    item.ClassList = JsonHelper.ToJson(course.ClassList.Where(t => t != string.Empty));
                list.Add(item);
            }
            var result = UnitOfWork.Transaction(() =>
            {
                ElectiveBatchRepository.Insert(model);
                ElectiveCourseRepository.Insert(list);
            });
            return DResult.FromResult(result);
        }

        public DResults<ElectiveBatchDto> List(string agencyId, DPage page = null)
        {
            if (string.IsNullOrWhiteSpace(agencyId))
                return DResult.Errors<ElectiveBatchDto>("学校ID异常！");
            page = page ?? DPage.NewPage();
            var count =
                ElectiveBatchRepository.Count(t => t.AgencyId == agencyId && t.Status != (byte)ElectiveStatus.Delete);
            var models = ElectiveBatchRepository.Where(
                t => t.AgencyId == agencyId && t.Status != (byte)ElectiveStatus.Delete)
                .OrderByDescending(t => t.CreationTime)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList()
                .MapTo<List<ElectiveBatchDto>>();
            return DResult.Succ(models, count);
        }

        private DResult UpdateStatus(string batch, ElectiveStatus status)
        {
            var model = ElectiveBatchRepository.Load(batch);
            if (model == null)
                return DResult.Error("选修课不存在！");
            if (model.Status == (byte)ElectiveStatus.Delete)
                return DResult.Error("选修课已被删除！");
            if (status == ElectiveStatus.Started)
            {
                var started =
                    ElectiveBatchRepository.Exists(
                        t => t.AgencyId == model.AgencyId && t.Status == (byte)ElectiveStatus.Started);
                if (started)
                    return DResult.Error("不能同时开启多个选课！");
            }
            model.Status = (byte)status;
            var result = ElectiveBatchRepository.Update(t => new
            {
                t.Status
            }, model);
            return DResult.FromResult(result);
        }

        /// <summary> 开启 </summary>
        public DResult Start(string batch)
        {
            return UpdateStatus(batch, ElectiveStatus.Started);
        }

        /// <summary> 关闭 </summary>
        public DResult Close(string batch)
        {
            return UpdateStatus(batch, ElectiveStatus.Finished);
        }

        /// <summary> 机构选修课 </summary>
        public string AgencyCourse(string agencyId, long userId)
        {
            if (string.IsNullOrWhiteSpace(agencyId) || userId < 0)
                return string.Empty;
            //判断是否是机构学生
            var groupResult = GroupContract.Groups(userId, (int)GroupType.Class);
            if (!groupResult.Status || groupResult.Data == null)
                return string.Empty;
            var groups =
                groupResult.Data.Where(t => t.CertificationLevel.HasValue && t.CertificationLevel.Value > 0 && t.AgencyId == agencyId).ToList();
            if (!groups.Any())
                return string.Empty;
            var batch =
                ElectiveBatchRepository.Where(
                    t => t.AgencyId == agencyId && t.Status == (byte)ElectiveStatus.Started)
                    .Select(t => t.Id)
                    .FirstOrDefault();
            return batch;
        }

        /// <summary> 选修课详情 </summary>
        /// <param name="batch"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public DResult<CourseDto> CourseList(string batch, long userId = 0)
        {
            Func<string, DResult<CourseDto>> errorFunc = DResult.Error<CourseDto>;
            var model = ElectiveBatchRepository.Load(batch);
            if (model == null || model.Status == (byte)ElectiveStatus.Delete)
                return errorFunc("选修课不存在！");
            if (userId > 0 && model.Status != (byte)ElectiveStatus.Started)
                return errorFunc("还未开始选课！");
            var dto = new CourseDto
            {
                Batch = batch,
                Title = model.Title,
                Courses = new List<CourseItemDto>()
            };
            var courseList =
                ElectiveCourseRepository.Where(t => t.Batch == batch && t.Status == (byte)NormalStatus.Normal);
            var selectedId = 0;
            if (userId > 0)
            {
                var list = ElectiveDetailRepository.Where(
                    d => d.Batch == batch && d.StudentId == userId && d.Status == (byte)NormalStatus.Normal)
                    .Select(d => d.CourseId)
                    .ToList();
                if (list.Any())
                    selectedId = list.First();
            }
            foreach (var course in courseList)
            {
                var item = new CourseItemDto
                {
                    Id = course.Id,
                    Name = course.CourseName,
                    Teacher = course.TeacherName,
                    Address = course.Address,
                    Capacity = course.TotalCapacity,
                    SelectedCount = course.SelectedCount,
                    ClassList = course.ClassList,
                    Selected = (course.Id == selectedId)
                };
                dto.Courses.Add(item);
            }
            dto.Selectable = (selectedId == 0);
            return DResult.Succ(dto);
        }

        /// <summary> 删除选修课 </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        public DResult Delete(string batch)
        {
            return UpdateStatus(batch, ElectiveStatus.Delete);
        }

        /// <summary> 选课 </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public DResult Course(int id, long studentId)
        {
            var model = ElectiveCourseRepository.Load(id);
            if (model == null)
                return DResult.Error("课程不存在！");
            if (model.SelectedCount >= model.TotalCapacity)
                return DResult.Error("该课程已选满！");
            if (
                ElectiveDetailRepository.Exists(
                    t => t.Batch == model.Batch && t.StudentId == studentId && t.Status == (byte)NormalStatus.Normal))
            {
                return DResult.Error("你已选了其他选修课，请先退课之后再选！");
            }
            var batchModel = ElectiveBatchRepository.Load(model.Batch);
            if (batchModel == null || batchModel.Status != (byte)ElectiveStatus.Started)
                return DResult.Error("还未开始选课！");
            //机构权限
            var student = UserContract.Load(studentId);
            if (!student.IsStudent())
                return DResult.Error("只有学生才能选课！");
            var groupResult = GroupContract.Groups(studentId, (int)GroupType.Class);
            if (!groupResult.Status || groupResult.Data == null)
                return DResult.Error("所在班级不能选该门课程！");
            //需要该机构认证过的班级圈权限
            var groups =
                groupResult.Data
                    .Where(g => g.CertificationLevel.HasValue && g.CertificationLevel.Value > 0 && g.AgencyId == batchModel.AgencyId)
                    .Select(g => g.Id).ToList();
            if (!groups.Any())
                return DResult.Error("所在班级不能选该门课程！");
            var classId = groups.First();
            //班级权限
            if (!string.IsNullOrWhiteSpace(model.ClassList))
            {
                var classList = JsonHelper.JsonList<string>(model.ClassList).ToList();
                var classItem = classList.Intersect(groups).ToList();
                if (!classItem.Any())
                    return DResult.Error("所在班级不能选该门课程！");
                classId = classItem.First();
            }
            if (model.ClassCapacity > 0)
            {
                var classSelectedCount = ElectiveDetailRepository.Count(
                    d => d.Batch == model.Batch && d.ClassId == classId && d.Status == (byte)NormalStatus.Normal);
                if (classSelectedCount >= model.ClassCapacity)
                {
                    return DResult.Error("该课程在班级内名额已满！");
                }
            }
            var detail = new TS_ElectiveDetail
            {
                Id = IdHelper.Instance.Guid32,
                Batch = model.Batch,
                CourseId = id,
                StudentId = studentId,
                ClassId = classId,
                Status = (byte)NormalStatus.Normal,
                AddedAt = Clock.Now
            };
            model.SelectedCount += 1;

            var result = UnitOfWork.Transaction(() =>
            {
                ElectiveDetailRepository.Insert(detail);
                ElectiveCourseRepository.Update(t => new
                {
                    t.SelectedCount
                }, model);
            });

            return DResult.FromResult(result);
        }

        /// <summary> 退课 </summary>
        /// <param name="id"></param>
        /// <param name="studentId"></param>
        /// <returns></returns>
        public DResult QuitCourse(int id, long studentId)
        {
            var courseModel = ElectiveCourseRepository.Load(id);
            if (courseModel == null)
                return DResult.Error("课程不存在！");
            var model =
                ElectiveDetailRepository.FirstOrDefault(
                    t => t.CourseId == id && t.StudentId == studentId && t.Status == (byte)NormalStatus.Normal);
            if (model == null)
                return DResult.Error("你还没有选修该课程！");
            var result = UnitOfWork.Transaction(() =>
            {
                courseModel.SelectedCount -= 1;
                if (courseModel.SelectedCount < 0)
                    courseModel.SelectedCount = 0;
                ElectiveCourseRepository.Update(t => new
                {
                    t.SelectedCount
                }, courseModel);
                ElectiveDetailRepository.Delete(model);
            });
            return DResult.FromResult(result);
        }

        /// <summary> 选课详情 </summary>
        /// <param name="batch"></param>
        public DResults<ElectiveDetailDto> Details(string batch)
        {
            var model = ElectiveBatchRepository.Load(batch);
            if (model == null || model.Status == (byte)ElectiveStatus.Delete)
                return DResult.Errors<ElectiveDetailDto>("选修课不存在！");
            if (model.Status != (byte)ElectiveStatus.Finished)
                return DResult.Errors<ElectiveDetailDto>("选修课尚未结束！");
            var courseList = ElectiveCourseRepository.Where(t => t.Batch == batch)
                .Select(t => new ElectiveDetailDto
                {
                    CourseId = t.Id,
                    CourseName = t.CourseName,
                    Teacher = t.TeacherName,
                    Address = t.Address,
                    Capacity = t.TotalCapacity,
                    Current = t.SelectedCount
                }).ToList();
            var details =
                ElectiveDetailRepository.Where(t => t.Batch == batch && t.Status == (byte)NormalStatus.Normal)
                    .Select(t => new { t.CourseId, t.StudentId, t.ClassId, t.AddedAt })
                    .ToList()
                    .GroupBy(t => t.CourseId)
                    .ToDictionary(k => k.Key, v => v.ToList());
            if (!details.Any())
                return DResult.Succ(courseList, -1);
            var userIds = details.Values.SelectMany(d => d.Select(t => t.StudentId)).Distinct();
            var userDict = UserContract.LoadListDictUser(userIds);
            var classList = details.Values.SelectMany(d => d.Select(t => t.ClassId)).Distinct().ToList();
            var classDict = GroupContract.GroupDict(classList);
            courseList.ForEach(t =>
            {
                if (!details.ContainsKey(t.CourseId))
                    return;
                var students = details[t.CourseId];
                t.Students = new List<ElectiveUserDto>();
                foreach (var student in students)
                {
                    var dto = new ElectiveUserDto
                    {
                        Id = student.StudentId,
                        Time = student.AddedAt
                    };
                    if (userDict.ContainsKey(dto.Id))
                        dto.Name = userDict[dto.Id].Name;
                    if (classDict.ContainsKey(student.ClassId))
                        dto.ClassNmae = classDict[student.ClassId];
                    t.Students.Add(dto);
                }
            });
            return DResult.Succ(courseList, -1);
        }
    }
}
