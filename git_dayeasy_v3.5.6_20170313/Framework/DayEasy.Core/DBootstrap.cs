
using System.Reflection;
using DayEasy.Core.Dependency;
using DayEasy.Core.Modules;

namespace DayEasy.Core
{
    public abstract class DBootstrap : IBootstrap
    {
        public abstract void Initialize(Assembly executingAssembly = null);

        protected bool IsDisposed;

        /// <summary> 注入管理 </summary>
        public IIocManager IocManager { get; protected set; }

        /// <summary> 注册依赖 </summary>
        public abstract void IocRegisters(Assembly executingAssembly);

        /// <summary> 初始化各个模块 </summary>
        public void ModulesInstaller()
        {
            IocManager.Resolve<IModuleManager>().InitializeModules();
        }

        /// <summary> 日志初始化 </summary>
        public abstract void LoggerInit();

        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            IocManager.Resolve<IModuleManager>().ShutdownModules();
        }
    }
}
