using DayEasy.Core;
using DayEasy.Core.Domain.Repositories;
using DayEasy.Ebook.Contracts;
using DayEasy.Ebook.Contracts.Models;

namespace DayEasy.Ebook.Services.Services
{
    public class EbookService : DService, IEbookContract
    {
        public EbookService(EbookDbContext unitOfWork)
            : base(unitOfWork)
        {
        }

        public IRepository<EbookDbContext, TE_TextBook, string> TextBooks { private get; set; }

        public IRepository<TE_TextBook, string> TextBookRepository
        {
            get { return TextBooks; }
        }
    }
}
