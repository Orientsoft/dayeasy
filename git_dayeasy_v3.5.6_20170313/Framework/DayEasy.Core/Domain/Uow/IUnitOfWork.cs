﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace DayEasy.Core.Domain.Uow
{
    /// <summary> 业务单元操作接口 </summary>
    public interface IUnitOfWork : IDependency
    {
        /// <summary> 是否开启事务提交 </summary>
        bool IsTransaction { get; set; }

        //        bool IsDisposed { get; }

        /// <summary> 执行sql语句 </summary>
        /// <param name="behavior"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int SqlExecute(TransactionalBehavior behavior, string sql, params object[] parameters);

        /// <summary> 执行sql查询 </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable<TEntity> SqlQuery<TEntity>(string sql, params object[] parameters);

        /// <summary> 执行sql查询 </summary>
        /// <param name="entityType"></param>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        IEnumerable SqlQuery(Type entityType, string sql, params object[] parameters);

        /// <summary> 保存到数据库 </summary>
        /// <returns></returns>
        int SaveChanges();

        /// <summary> 执行事务操作 </summary>
        /// <param name="action"></param>
        int Transaction(Action action);

        /// <summary> 执行事务操作 </summary>
        /// <param name="action"></param>
        int Transaction(Action<IUnitOfWork> action);
    }
}
