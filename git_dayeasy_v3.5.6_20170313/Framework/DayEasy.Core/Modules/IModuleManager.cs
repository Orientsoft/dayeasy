﻿
namespace DayEasy.Core.Modules
{
    /// <summary> 模块管理接口 </summary>
    public interface IModuleManager : ILifetimeDependency
    {
        /// <summary> 加载所有模块 </summary>
        void InitializeModules();

        /// <summary> 关闭所有模块 </summary>
        void ShutdownModules();
    }
}
