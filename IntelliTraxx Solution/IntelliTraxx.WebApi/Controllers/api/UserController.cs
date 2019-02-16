
using System.Web.Http;
using IntelliTraxx.Shared.TruckService;
using IntelliTraxx.WebApi.Helpers;

namespace IntelliTraxx.WebApi.Controllers.api
{
    [Authorize]
    public class UserController : ApiBaseController
    {
        private readonly TruckServiceClient _truckService = new TruckServiceClient();

        public UserController(IAppBaseContracts appBaseContracts)
        : base(appBaseContracts)
        {

        }

        [HttpGet]
        [Route("api/user/getUserContext")]
        public IHttpActionResult GetUserContext()
        {
            var userSecurityContext = UserSecurity.GetCurrentUserSecurityContext();
            var user = _truckService.getUserProfile(userSecurityContext.Id);
            if (user == null)
                return BadRequest("User not found");

            var returnItem = new
            {
                user
            };

            return Ok(returnItem);
        }
    }
}
