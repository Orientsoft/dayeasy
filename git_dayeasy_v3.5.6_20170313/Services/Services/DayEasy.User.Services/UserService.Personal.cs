using System.Collections.Generic;
using System.Linq;
using DayEasy.Contracts.Dtos.User;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Models;
using DayEasy.Core.Domain;
using DayEasy.Services;
using DayEasy.User.Services.Helper;
using DayEasy.Utility;
using DayEasy.Utility.Extend;
using DayEasy.Utility.Helper;
using DayEasy.Utility.Timing;

namespace DayEasy.User.Services
{
    public partial class UserService
    {
        public IDayEasyRepository<TU_Impression> ImpressionRepository { private get; set; }
        public IDayEasyRepository<TU_Quotations> QuotationsRepository { private get; set; }
        public IDayEasyRepository<TU_ImpressionLike> ImpressionLikeRepository { private get; set; }
        public IDayEasyRepository<TU_Visit> VisitRepository { private get; set; }

        public int Visit(long userId, long visitorId = 0)
        {
            var user = UserRepository.Load(userId);
            if (user == null)
                return 0;
            if (userId == visitorId)
                return user.VisitCount;
            user.VisitCount++;
            UserRepository.Update(user, "VisitCount");
            if (visitorId <= 0)
                return user.VisitCount;
            var visit = VisitRepository.FirstOrDefault(t => t.UserId == userId && t.VisitUserId == visitorId);
            if (visit == null || visit.VisitTime < Clock.Now.AddMinutes(-1))
            {
                VisitRepository.Insert(new TU_Visit
                {
                    Id = IdHelper.Instance.Guid32,
                    UserId = userId,
                    VisitUserId = visitorId,
                    VisitTime = Clock.Now
                });
            }
            else
            {
                visit.VisitTime = Clock.Now;
                VisitRepository.Update(visit, "VisitTime");
            }
            return user.VisitCount;
        }

        public DResult EditSignature(long userId, string signature)
        {
            var result = UserRepository.Update(new TU_User
            {
                Signature = signature
            }, u => u.Id == userId, "Signature");
            if (result > 0)
            {
                UserCache.Instance.Remove(userId);
            }
            return DResult.FromResult(result);
        }

        public DResult AddImpression(ImpressionInputDto dto)
        {
            if (dto == null || dto.UserId <= 0 || dto.CreatorId <= 0)
                return DResult.Error("数据异常,添加失败！");
            if (dto.Content.IsNullOrEmpty())
                return DResult.Error("没有任何贴纸信息！");
            var helper = IdHelper.Instance;
            dto.Content = dto.Content.Distinct().ToList();
            var insertList = new List<TU_Impression>();
            var updateList = new List<TU_Impression>();
            var likeList = new List<TU_ImpressionLike>();
            foreach (var content in dto.Content)
            {
                if (string.IsNullOrWhiteSpace(content))
                    continue;
                var item = content;
                var impression =
                    ImpressionRepository.FirstOrDefault(t => t.UserId == dto.UserId && t.Impression == item);
                if (impression != null)
                {
                    if (ImpressionLikeRepository.Exists(
                        t => t.ContentId == impression.Id && t.UserId == dto.CreatorId))
                        continue;
                    impression.GoodsCount++;
                    likeList.Add(new TU_ImpressionLike
                    {
                        Id = IdHelper.Instance.Guid32,
                        ContentId = impression.Id,
                        UserId = dto.CreatorId,
                        CreationTime = Clock.Now
                    });
                    updateList.Add(impression);
                }
                else
                {
                    insertList.Add(new TU_Impression
                    {
                        Id = helper.Guid32,
                        UserId = dto.UserId,
                        Impression = item,
                        CreatorId = dto.CreatorId,
                        CreationTime = Clock.Now,
                        GoodsCount = 0
                    });
                }
            }
            if (!insertList.Any() && !updateList.Any())
                return DResult.Success;
            var result = ImpressionRepository.UnitOfWork.Transaction(() =>
            {
                if (updateList.Any())
                {
                    ImpressionRepository.Update(t => new
                    {
                        t.GoodsCount
                    }, updateList.ToArray());
                }
                if (likeList.Any())
                {
                    ImpressionLikeRepository.Insert(likeList.ToArray());
                }
                if (insertList.Any())
                {
                    ImpressionRepository.Insert(insertList.ToArray());
                }
            });
            return DResult.FromResult(result);
        }

        public DResult SupportImpression(string id, long userId)
        {
            var model = ImpressionRepository.Load(id);
            if (model == null)
                return DResult.Error("贴纸不存在!");
            var item = ImpressionLikeRepository.FirstOrDefault(t => t.ContentId == id && t.UserId == userId);
            if (item != null)
                return DResult.Error("已支持过了！");
            var result = ImpressionRepository.UnitOfWork.Transaction(() =>
            {
                ImpressionLikeRepository.Insert(new TU_ImpressionLike
                {
                    Id = IdHelper.Instance.Guid32,
                    ContentId = id,
                    UserId = userId,
                    CreationTime = Clock.Now
                });
                model.GoodsCount++;
                ImpressionRepository.Update(model, "GoodsCount");
            });
            return DResult.FromResult(result);
        }

        public DResult CancelSupportImpression(string id, long userId)
        {
            var model = ImpressionRepository.Load(id);
            if (model == null)
                return DResult.Error("贴纸不存在!");
            var item = ImpressionLikeRepository.FirstOrDefault(t => t.ContentId == id && t.UserId == userId);
            if (item == null)
                return DResult.Error("已取消支持！");
            var result = ImpressionRepository.UnitOfWork.Transaction(() =>
            {
                ImpressionLikeRepository.Delete(item);
                model.GoodsCount--;
                ImpressionRepository.Update(model, "GoodsCount");
            });
            return DResult.FromResult(result);
        }

        public DResult DeleteImpression(string id, long userId)
        {
            var model = ImpressionRepository.Load(id);
            if (model == null)
                return DResult.Error("贴纸不存在!");
            if (model.CreatorId != userId && model.UserId != userId)
                return DResult.Error("不能删除其他人的贴纸!");
            var result = ImpressionRepository.UnitOfWork.Transaction(() =>
            {
                ImpressionLikeRepository.Delete(t => t.ContentId == id);
                ImpressionRepository.Delete(model);
            });
            return DResult.FromResult(result);
        }

        public DResults<ImpressionDto> ImpressionList(long userId, DPage page)
        {
            if (userId <= 0)
                return DResult.Errors<ImpressionDto>("用户ID异常！");
            var list = new List<ImpressionDto>();
            var models = ImpressionRepository.Where(t => t.UserId == userId)
                .OrderByDescending(t => t.GoodsCount)
                .ThenBy(t => t.Impression)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            if (!models.Any())
                return DResult.Succ(list, 0);
            var ids = models.Select(t => t.Id).ToList();
            var supports = ImpressionLikeRepository.Where(t => ids.Contains(t.ContentId))
                .Select(t => new { t.ContentId, t.UserId, t.CreationTime })
                .ToList()
                .GroupBy(t => t.ContentId)
                .ToDictionary(k => k.Key, v => v.OrderByDescending(t => t.CreationTime).Select(t => t.UserId).ToList());
            foreach (var model in models)
            {
                var dto = new ImpressionDto
                {
                    Id = model.Id,
                    CreatorId = model.CreatorId,
                    Content = model.Impression.HtmlDecode(),
                };
                if (supports.ContainsKey(dto.Id))
                {
                    dto.SupportList = supports[dto.Id];
                }
                list.Add(dto);
            }
            var count = ImpressionRepository.Count(t => t.UserId == userId);
            return DResult.Succ(list, count);
        }

        public List<string> LastImpressions(long userId, long visitorId)
        {
            if (userId <= 0 || visitorId <= 0)
                return null;
            var lastTime = VisitRepository.Where(t => t.UserId == userId && t.VisitUserId == visitorId)
                .Select(t => t.VisitTime)
                .OrderByDescending(t => t)
                .FirstOrDefault();
            var list =
                ImpressionRepository.Where(
                    t =>
                        t.UserId == userId && t.CreatorId != visitorId &&
                        (lastTime == null || t.CreationTime > lastTime))
                    .OrderByDescending(t => t.CreationTime)
                    .Select(t => t.Impression)
                    .Take(10)
                    .ToList();
            return list;
        }

        public List<string> HotImpressions(UserRole role, int count)
        {
            var impressions = UserRepository.Where(u => (u.Role & (byte)role) > 0)
                .Join(ImpressionRepository.Table, u => u.Id, i => i.UserId, (u, i) => new
                {
                    i.Impression,
                    i.GoodsCount
                })
                .GroupBy(t => t.Impression)
                .OrderByDescending(t => t.Sum(a => a.GoodsCount))
                .Select(t => t.Key)
                .Take(count)
                .ToList();
            return impressions;
        }


        public DResult AddQuotations(QuotationsInputDto dto)
        {
            if (dto == null || dto.UserId <= 0 || dto.CreatorId <= 0)
                return DResult.Error("数据异常,添加失败！");
            if (dto.Content.IsNullOrEmpty())
                return DResult.Error("没有任何语录信息！");
            dto.Content = dto.Content.HtmlEncode();
            var helper = IdHelper.Instance;
            var item = new TU_Quotations
            {
                Id = helper.Guid32,
                UserId = dto.UserId,
                Content = dto.Content,
                CreatorId = dto.CreatorId,
                CreationTime = Clock.Now,
                GoodsCount = 0
            };
            var result = QuotationsRepository.Insert(item);
            return string.IsNullOrWhiteSpace(result) ? DResult.Error("添加失败！") : DResult.Success;
        }

        public DResult SupportQuotations(string id, long userId)
        {
            var model = QuotationsRepository.Load(id);
            if (model == null)
                return DResult.Error("语录不存在!");
            var item = ImpressionLikeRepository.FirstOrDefault(t => t.ContentId == id && t.UserId == userId);
            if (item != null)
                return DResult.Error("已支持过了！");
            var result = ImpressionRepository.UnitOfWork.Transaction(() =>
            {
                ImpressionLikeRepository.Insert(new TU_ImpressionLike
                {
                    Id = IdHelper.Instance.Guid32,
                    ContentId = id,
                    UserId = userId,
                    CreationTime = Clock.Now
                });
                model.GoodsCount++;
                QuotationsRepository.Update(model, "GoodsCount");
            });
            return DResult.FromResult(result);
        }

        public DResult CancelSupportQuotations(string id, long userId)
        {
            var model = QuotationsRepository.Load(id);
            if (model == null)
                return DResult.Error("语录不存在!");
            var item = ImpressionLikeRepository.FirstOrDefault(t => t.ContentId == id && t.UserId == userId);
            if (item == null)
                return DResult.Error("已取消支持！");
            var result = ImpressionRepository.UnitOfWork.Transaction(() =>
            {
                ImpressionLikeRepository.Delete(item);
                model.GoodsCount--;
                QuotationsRepository.Update(model, "GoodsCount");
            });
            return DResult.FromResult(result);
        }

        public DResult DeleteQuotations(string id, long userId)
        {
            var model = QuotationsRepository.Load(id);
            if (model == null)
                return DResult.Error("贴纸不存在!");
            if (model.CreatorId != userId && model.UserId != userId)
                return DResult.Error("不能删除其他人的贴纸!");
            var result = QuotationsRepository.UnitOfWork.Transaction(() =>
            {
                ImpressionLikeRepository.Delete(t => t.ContentId == id);
                QuotationsRepository.Delete(model);
            });
            return DResult.FromResult(result);
        }

        public DResults<QuotationsDto> QuotationsList(long userId, DPage page)
        {
            if (userId <= 0)
                return DResult.Errors<QuotationsDto>("用户ID异常！");
            var list = new List<QuotationsDto>();
            var models = QuotationsRepository.Where(t => t.UserId == userId)
                .OrderByDescending(t => t.GoodsCount)
                .ThenBy(t => t.CreationTime)
                .Skip(page.Page * page.Size)
                .Take(page.Size)
                .ToList();
            if (!models.Any())
                return DResult.Succ(list, 0);
            var ids = models.Select(t => t.Id).ToList();
            var supports = ImpressionLikeRepository.Where(t => ids.Contains(t.ContentId))
                .Select(t => new { t.ContentId, t.UserId, t.CreationTime })
                .ToList()
                .GroupBy(t => t.ContentId)
                .ToDictionary(k => k.Key, v => v.OrderByDescending(t => t.CreationTime).Select(t => t.UserId).ToList());
            var userIds = models.Select(t => t.CreatorId).Distinct().ToList();
            var users = LoadList(userIds);
            foreach (var model in models)
            {
                var dto = new QuotationsDto
                {
                    Id = model.Id,
                    CreatorId = model.CreatorId,
                    CreationTime = model.CreationTime,
                    UserId = model.UserId,
                    Content = model.Content.FormatMessage().HtmlDecode()
                };
                var user = users.FirstOrDefault(u => u.Id == dto.CreatorId);
                if (user != null)
                {
                    dto.UserName = user.Name;
                    dto.UserCode = user.Code;
                }

                if (supports.ContainsKey(dto.Id))
                {
                    dto.SupportList = supports[dto.Id];
                }
                list.Add(dto);
            }
            var count = QuotationsRepository.Count(t => t.UserId == userId);
            return DResult.Succ(list, count);
        }
    }
}
