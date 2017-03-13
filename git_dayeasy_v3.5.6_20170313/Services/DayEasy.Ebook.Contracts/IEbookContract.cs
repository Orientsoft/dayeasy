using DayEasy.Core;
using DayEasy.Core.Domain.Repositories;
using DayEasy.Ebook.Contracts.Models;

namespace DayEasy.Ebook.Contracts
{
    /// <summary> 电子课本业务模块 </summary>
    public interface IEbookContract : IDependency
    {
        IRepository<TE_TextBook, string> TextBookRepository { get; }
    }
}
