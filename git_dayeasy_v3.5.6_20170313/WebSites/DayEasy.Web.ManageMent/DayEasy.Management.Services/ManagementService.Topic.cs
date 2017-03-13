using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DayEasy.Contracts.Enum;
using DayEasy.Contracts.Management.Dto;
using DayEasy.Contracts.Models;
using DayEasy.Services;
using DayEasy.Utility;

namespace DayEasy.Management.Services
{
    public partial class ManagementService
    {
        public IVersion3Repository<TB_Topic, string> TopicRepository { private get; set; }

        public DResults<TB_Topic> GetTopics(TopicSearchDto searchDto)
        {
            if (searchDto == null)
                return DResult.Errors<TB_Topic>("参数错误！");

            var topics = TopicRepository.Table;

            if (searchDto.ClassType > -1)
            {
                if (searchDto.ClassType % 100 == 0)//顶级
                {
                    var type = searchDto.ClassType / 100;

                    topics = topics.Where(u => u.ClassType >= type * 100 && u.ClassType < (type + 1) * 100);
                }
                else
                {
                    topics = topics.Where(u => u.ClassType == searchDto.ClassType);
                }
            }

            if (!string.IsNullOrEmpty(searchDto.KeyWord))
            {
                topics = topics.Where(u => u.Title.Contains(searchDto.KeyWord));
            }

            if (searchDto.TopicStatus > -1)
            {
                topics = topics.Where(u => u.Status == (byte)searchDto.TopicStatus);
            }

            if (searchDto.Auth > -1)
            {
                var groups = ShareRepository.Where(u => u.JoinAuth == (byte)searchDto.Auth);

                topics = topics.Join(groups, t => t.GroupId, g => g.Id, (t, g) => t);
            }

            switch (searchDto.Sort)
            {
                case "read":
                    topics = topics.OrderByDescending(u => u.ReadNum);
                    break;
                case "praise":
                    topics = topics.OrderByDescending(u => u.PraiseNum);
                    break;
                case "reply":
                    topics = topics.OrderByDescending(u => u.ReplyNum);
                    break;
                default:
                    topics = topics.OrderByDescending(u => u.AddedAt);
                    break;
            }

            var result = topics.Skip(searchDto.Page * searchDto.Size)
                .Take(searchDto.Size)
                .ToList();

            return DResult.Succ(result, topics.Count());
        }


        public DResult UpdateTopicStatus(string topicId, TopicStatus status)
        {
            if (string.IsNullOrEmpty(topicId))
                return DResult.Error("参数错误！");

            var topic = TopicRepository.SingleOrDefault(u => u.Id == topicId);
            if (topic == null) return DResult.Error("没有找到该帖子！");

            if (topic.Status == (byte)status) return DResult.Success;

            var share = ShareRepository.SingleOrDefault(u => u.Id == topic.GroupId);
            if (share != null)
            {
                if (topic.Status == (byte)TopicStatus.Delete)
                {
                    share.TopicNum++;
                }
                else if ((byte)status == (byte)TopicStatus.Delete)
                {
                    share.TopicNum--;
                }
            }

            topic.Status = (byte)status;
            var result = TopicRepository.UnitOfWork.Transaction(() =>
            {
                TopicRepository.Update(topic);
                if (share != null)
                {
                    ShareRepository.Update(share);
                }
            });

            return result > 0 ? DResult.Success : DResult.Error("操作失败，请稍后重试！");
        }
    }
}
