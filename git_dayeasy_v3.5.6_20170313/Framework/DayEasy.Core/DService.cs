using DayEasy.Core.Domain.Uow;

namespace DayEasy.Core
{
    /// <summary> 服务基类 </summary>
    public abstract class DService
    {
        protected DService(IUnitOfWork unitOfWork)
        {
            UnitOfWork = unitOfWork;
        }

        /// <summary> 单元操作 </summary>
        public IUnitOfWork UnitOfWork { get; private set; }
    }
}
