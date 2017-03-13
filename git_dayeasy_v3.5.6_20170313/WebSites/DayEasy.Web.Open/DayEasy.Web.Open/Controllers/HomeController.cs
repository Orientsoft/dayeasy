using System.Web;
using System.Web.Http;
using DayEasy.Contracts;
using DayEasy.Contracts.Dtos.Question;
using DayEasy.Utility;
using DayEasy.Utility.Timing;
using DayEasy.Web.Api;
using DayEasy.Web.Api.Attributes;
using DayEasy.Web.Api.Config;

namespace DayEasy.Web.Open.Controllers
{
    public class HomeController : ApiController
    {
        private readonly IPaperContract _paperContract;
        public HomeController(IPaperContract paperContract)
        {
            _paperContract = paperContract;
        }

        [JsonCallback]
        [HttpGet]
        public QuestionDto Load(string id)
        {
            return _paperContract.LoadQuestion(id);
        }

        [HttpGet]
        [Route("~/ticks")]
        public long Ticks()
        {
            return Clock.Now.ToLong();
        }

        [HttpGet]
        [Route("~/download_url")]
        public string DownloadUrl(int type)
        {
            var manifest = ManifestInfo.Get(type);
            if (manifest != null)
                return manifest.DownloadUrl;
            return string.Empty;
        }

        [HttpGet]
        [Route("~/download")]
        public DResult Download(int type)
        {
            var manifest = ManifestInfo.Get(type);
            if (manifest != null)
            {
                HttpContext.Current.Response.Redirect(manifest.DownloadUrl, true);
            }
            return DResult.Success;
        }
    }
}
