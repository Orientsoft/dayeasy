using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Caching;

namespace DayEasy.Web.Static.Handler
{
    /// <summary> 合并资源协定 </summary>
    public class CombineHandler : IHttpHandler
    {
        private HttpContext _context;
        private string _cacheKey;
        private string _hash;

        private List<string> _pathList = new List<string>();

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            _context = context;
            var rawUrl = _context.Request.Url.Query;
            if (string.IsNullOrWhiteSpace(rawUrl)) return;
            rawUrl = rawUrl.TrimStart('?').Split('&')[0];
            _pathList = rawUrl.Split(new[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (!_pathList.Any()) return;
            CombineMission();
        }

        private void CombineMission()
        {
            var sb = new StringBuilder();
            string ext = Path.GetExtension(_pathList.First().Split('?')[0]),
                contentType = WebHelper.GetContentType(ext);
            _cacheKey = _context.Request.Url.AbsoluteUri;
            _hash = WebHelper.GetMd5Sum(_cacheKey);

            if (WebHelper.IsCachedOnBrowser(_context, _hash, contentType))
                return;

            if (_context.Cache[_cacheKey] == null)
            {
                var fileNames = new List<string>();
                foreach (string file in _pathList)
                {
                    string temp = WebHelper.GetLocalFile(_context, file, fileNames);
                    if (!string.IsNullOrWhiteSpace(temp))
                    {
                        sb.AppendLine(temp);
                    }
                }

                if (fileNames.Count > 0)
                    _context.Cache.Insert(_cacheKey, sb, new CacheDependency(fileNames.ToArray()));
                else
                    _context.Cache.Insert(_cacheKey, sb, null, Cache.NoAbsoluteExpiration,
                        new TimeSpan(3, 0, 0, 0));
            }
            ResponseTo(contentType);
        }

        private void ResponseTo(string contentType)
        {
            var rep = _context.Response;
            rep.ClearHeaders();
            rep.AppendHeader("Vary", "Accept-Encoding");
            rep.AppendHeader("Cache-Control", "max-age=604800");
            rep.AppendHeader("Expires", DateTime.Now.AddYears(1).ToString("R"));
            rep.AppendHeader("ETag", _hash);
            rep.AppendHeader("Content-Type", contentType);
            rep.AppendHeader("Content-Encoding", "gzip");
            rep.Filter = new GZipStream(rep.Filter, CompressionMode.Compress);
            rep.Write(_context.Cache[_cacheKey]);
        }
    }
}
