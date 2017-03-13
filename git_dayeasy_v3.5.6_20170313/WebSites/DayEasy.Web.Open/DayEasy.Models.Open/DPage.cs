
namespace DayEasy.Models.Open
{
    public class DPage : DDto
    {
        public int Page { get; set; }
        public int Size { get; set; }

        public DPage(int page = 0, int size = 12)
        {
            Page = page;
            Size = size;
        }

        public static DPage NewPage(int page = 0, int size = 12)
        {
            return new DPage(page, size);
        }
    }
}
