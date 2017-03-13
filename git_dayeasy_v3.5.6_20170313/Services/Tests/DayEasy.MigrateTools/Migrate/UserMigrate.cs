using System;
using System.Linq;
using Autofac;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Group;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.Utility.Helper;

namespace DayEasy.MigrateTools.Migrate
{
    public class UserMigrate : MigrateBase
    {
        private readonly ITempOldContract _tempOldContract;
        private readonly IGroupContract _groupContract;
        private readonly IDayEasyRepository<TU_User, long> _userRepository;
        private readonly IDayEasyRepository<TU_UserAgency> _userAgencyRepository;
        //        private IVersion3Repository<TG_Member> _groupRepository;

        public UserMigrate()
        {
            _tempOldContract = Container.Resolve<ITempOldContract>();
            _groupContract = Container.Resolve<IGroupContract>();
            _userRepository = Container.Resolve<IDayEasyRepository<TU_User, long>>();
            _userAgencyRepository = Container.Resolve<IDayEasyRepository<TU_UserAgency>>();
        }

        /// <summary> 生成得一号 </summary>
        public void GenerateDCode()
        {
            var result = _tempOldContract.UpdateDCode();
            Console.WriteLine(result);
        }

        public void UserAgency()
        {
            var users =
                _userRepository.Where(
                    u =>
                        (u.Status == (byte)UserStatus.Normal || u.Status == (byte)UserStatus.UnBind) &&
                        u.AgencyId == null)
                    .Select(u => u.Id)
                    .ToList();
            var len = users.Count;
            Console.WriteLine("用户数：{0}", len);
            int index = 0;
            foreach (var userId in users)
            {
                var id = userId;
                var user = _userRepository.Load(id);
                if (user == null || !string.IsNullOrWhiteSpace(user.AgencyId))
                    continue;
                var groupResult = _groupContract.Groups(id);
                if (!groupResult.Status || groupResult.TotalCount <= 0)
                    continue;
                var group =
                    groupResult.Data.OrderBy(g => g.Type).FirstOrDefault(
                        g => g.Type == (byte)GroupType.Class || g.Type == (byte)GroupType.Colleague);
                if (group == null)
                    continue;
                DateTime start;
                byte stage;
                string agencyId;
                var role = ((user.Role & (byte)UserRole.Teacher) > 0
                    ? (byte)UserRole.Teacher
                    : (byte)UserRole.Student);

                if (@group.Type == (byte)GroupType.Class)
                {
                    var classGroup = (ClassGroupDto)@group;
                    agencyId = classGroup.AgencyId;
                    start = new DateTime(classGroup.GradeYear, 9, 1);
                    stage = classGroup.Stage;
                }
                else
                {
                    var colleague = (ColleagueGroupDto)group;
                    agencyId = colleague.AgencyId;
                    start = colleague.CreationTime;
                    stage = colleague.Stage;
                }

                user.AgencyId = agencyId;

                var result = _userRepository.UnitOfWork.Transaction(() =>
                {
                    _userRepository.Update(user, "AgencyId");

                    _userAgencyRepository.Insert(new TU_UserAgency
                    {
                        Id = IdHelper.Instance.Guid32,
                        AgencyId = agencyId,
                        Status = (byte)UserAgencyStatus.Current,
                        UserId = id,
                        Stage = stage,
                        Role = role,
                        StartTime = start
                    });
                });
                index++;
                if (index % 500 == 0 || index == len)
                {
                    Console.WriteLine("已同步完成：[{0}/{1}]", index, len);
                }
            }
            Console.WriteLine("同步完成^_^");
        }
    }
}
