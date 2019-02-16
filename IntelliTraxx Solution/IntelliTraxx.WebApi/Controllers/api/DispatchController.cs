using System;
using System.Web.Http;
using IntelliTraxx.WebApi.Helpers;

namespace IntelliTraxx.WebApi.Controllers.api
{
   
    public class DispatchController : ApiBaseController
    {
        public DispatchController(IAppBaseContracts appBaseContracts)
        : base(appBaseContracts)
        {

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/dispatch/getServerTime")]
        public IHttpActionResult GetServerTime()
        {
            var data = DateTime.Now;
            return Ok(data);
        }

        [Authorize]
        [HttpGet]
        [Route("api/dispatch/getServerTimeSafe")]
        public IHttpActionResult GetServerTimeSafe()
        {
            var userSecurityContext = this.UserSecurity.GetCurrentUserSecurityContext();            
            var data = $"{DateTime.Now}-{userSecurityContext.FullName}-{userSecurityContext.Email}";
            return Ok(data);
        }
    }
}
