using System.IO;

namespace DayEasy.Office.Models
{
    public class StreamFile
    {
        //文件名 "1.docx"
        public string FileName { get; set; }
        //文件流
        public Stream Stream { get; set; }
    }
}
