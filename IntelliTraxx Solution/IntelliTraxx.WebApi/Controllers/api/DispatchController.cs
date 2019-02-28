using System;
using System.Linq;
using System.Web.Http;
using IntelliTraxx.Shared.TabletService;
using IntelliTraxx.Shared.TruckService;
using IntelliTraxx.WebApi.Helpers;

namespace IntelliTraxx.WebApi.Controllers.api
{
    [Authorize]    
    public class DispatchController : ApiBaseController
    {
        private readonly TruckServiceClient _truckService = new TruckServiceClient();
        private readonly TabletInterfaceClient _tabletInterfaceClient = new TabletInterfaceClient();

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

        [HttpGet]
        [Route("api/dispatch/getDispatchRequests/{vehicleId}")]
        public IHttpActionResult GetDispatchRequests(string vehicleId)
        {
            var data = _truckService.getDispatchesByVehicle(vehicleId).ToList();
            return Ok(data);
        }

        [HttpPost]
        [Route("api/dispatch/ackDispatch")]
        public IHttpActionResult AckDispatch(DispatchAck ack)
        {
            try
            {
                _tabletInterfaceClient.ackDispatch(ack.DispatchId, ack.Note, ack.DriverPin);
                return Ok();
            }
            catch (Exception e)
            {
                return this.Ok();
            }           
        }
    }

    public class DispatchAck
    {
        public Guid DispatchId { get; set; }
        public string Note { get; set; }
        public string DriverPin { get; set; }
    }
}
