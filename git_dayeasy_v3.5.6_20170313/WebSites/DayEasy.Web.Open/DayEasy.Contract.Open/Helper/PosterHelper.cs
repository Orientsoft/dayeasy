using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DayEasy.Contract.Open.Dtos;
using DayEasy.Core;
using DayEasy.Utility.Helper;

namespace DayEasy.Contract.Open.Helper
{
    public class PosterHelper
    {

        /// <summary> 制作海报，合成图片 </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public static string MakePoster(VTeacherGodInputDto dto)
        {
            string imgData = dto.ImageData, fileName;
            if (imgData.IndexOf("data:image/png", StringComparison.Ordinal) >= 0)
            {
                imgData = imgData.Replace("data:image/png;base64,", string.Empty);
                fileName = IdHelper.Instance.Guid32 + ".png";
            }
            else
            {
                imgData = imgData.Replace("data:image/jpeg;base64,", string.Empty);
                fileName = IdHelper.Instance.Guid32 + ".jpg";
            }
            var arr = Convert.FromBase64String(imgData);
            using (var imageMs = new MemoryStream(arr))
            {
                var fileDict = new Dictionary<string, Stream>
                {
                    {fileName, imageMs}
                };
                using (
                    var http = new HttpHelper(Consts.Config.FileSite + "/uploader?type=1", "POST", Encoding.UTF8,
                        string.Empty))
                {
                    //上传图片
                    http.AddFiles(fileDict);
                    var html = http.GetHtml();
                    if (string.IsNullOrEmpty(html))
                        return null;
                    var result = JsonHelper.Json<UploaderResultDto>(html);
                    if (result == null)
                        return null;
                    return result.urls.FirstOrDefault();
                }
            }
        }
    }
}
