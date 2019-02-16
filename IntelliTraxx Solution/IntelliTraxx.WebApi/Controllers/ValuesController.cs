using System.Web.Http;

namespace IntelliTraxx.WebApi.Controllers
{
    public class ValuesController : ApiController
    {
        
        [Route("api/values/getPublic")]
        public string GetPublic()
        {
            return "Public Data";
        }

        [Route("api/values/getSecure")]
        public string GetSecure()
        {
            return "Safe Data";
        }

    }
}
