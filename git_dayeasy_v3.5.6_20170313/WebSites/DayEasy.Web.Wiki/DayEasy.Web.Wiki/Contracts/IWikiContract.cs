﻿using System.Collections.Generic;
using DayEasy.Core;
using DayEasy.Core.Domain.Repositories;
using DayEasy.Utility;
using DayEasy.Web.Wiki.Models;
using DayEasy.Web.Wiki.Models.Dtos;

namespace DayEasy.Web.Wiki.Contracts
{
    public interface IWikiContract : IDependency
    {
        IRepository<WikiGroup, string> GroupRepository { get; }
        IRepository<Models.Wiki, string> WikiRepository { get; }

        /// <summary> 添加词条分类 </summary>
        /// <param name="name"></param>
        /// <param name="code"></param>
        /// <param name="creatorId"></param>
        /// <param name="logo"></param>
        /// <returns></returns>
        DResult AddGroup(string name, string code, string creatorId, string logo = null);

        /// <summary> 添加词条 </summary>
        /// <param name="name"></param>
        /// <param name="groupId"></param>
        /// <param name="creatorId"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        DResult<Models.Wiki> AddWiki(string name, string groupId, string creatorId, string desc);

        /// <summary> 添加词条详情 </summary>
        /// <param name="wikiId"></param>
        /// <param name="detail"></param>
        /// <returns></returns>
        DResult AddDetail(string wikiId, DetailDto detail);

        /// <summary> 批量添加词条详情 </summary>
        /// <param name="wikiId"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        DResult AddDetails(string wikiId, List<DetailDto> details);

        /// <summary> 获取所有词条分类 </summary>
        /// <returns></returns>
        List<GroupDto> GroupList();

        /// <summary> 加载词条 </summary>
        /// <param name="wiki"></param>
        /// <returns></returns>
        WikiDto Load(string wiki);
    }
}