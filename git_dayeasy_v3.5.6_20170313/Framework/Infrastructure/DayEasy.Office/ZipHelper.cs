using DayEasy.Office.Models;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DayEasy.Office
{
    /// <summary> 压缩文件辅助 </summary>
    public class ZipHelper
    {
        /// <summary> 创建压缩包 </summary>
        public static MemoryStream CreateZip(List<StreamFile> files)
        {
            if (files == null || !files.Any()) return null;
            var crc = new Crc32();
            var result = new MemoryStream();
            var zip = new ZipOutputStream(result);
            try
            {
                zip.SetLevel(9);
                files.ForEach(f =>
                {
                    var item = f.Stream;
                    item.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[item.Length];
                    item.Read(buffer, 0, buffer.Length);
                    item.Close();

                    crc.Reset();
                    crc.Update(buffer);
                    var entry = new ZipEntry(f.FileName) { Crc = crc.Value, DateTime = DateTime.Now };

                    zip.PutNextEntry(entry);
                    zip.Write(buffer, 0, buffer.Length);
                });
            }
            finally
            {
                zip.Flush();
                zip.Close();
                zip.Dispose();
            }
            return result;
        }
    }
}
