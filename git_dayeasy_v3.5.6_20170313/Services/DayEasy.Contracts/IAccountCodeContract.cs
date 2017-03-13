using DayEasy.Contracts.Models.Mongo;
using DayEasy.Core;

namespace DayEasy.Contracts
{
    /// <summary>
    /// 终端接收验证码记录
    /// </summary>
    public interface IAccountCodeContract : IDependency
    {
        /// <summary> 更新 </summary>
        void Edit(string account);

        /// <summary> 重置 </summary>
        void Reset(string account);

        /// <summary> 获取 </summary>
        MongoAccountCode Get(string account);
    }
}
