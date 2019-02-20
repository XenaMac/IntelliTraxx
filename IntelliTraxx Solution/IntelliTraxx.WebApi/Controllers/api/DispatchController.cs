using System.Web.Http;
using IntelliTraxx.Shared.TruckService;
using IntelliTraxx.WebApi.Helpers;

namespace IntelliTraxx.WebApi.Controllers.api
{
    [Authorize]
    public class DispatchController : ApiBaseController
    {
        private readonly TruckServiceClient _truckService = new TruckServiceClient();

        public DispatchController(IAppBaseContracts appBaseContracts)
        : base(appBaseContracts)
        {

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("api/dispatch/getAllVehicles/{includeHistorical}")]
        public IHttpActionResult GetAllVehicles(bool includeHistorical)
        {
            var data = _truckService.getAllVehicles(includeHistorical);
            return Ok(data);
        }
    }
}
