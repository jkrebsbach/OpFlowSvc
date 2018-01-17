using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class SurgeryController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetSurgery")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(Surgery))]
        public HttpResponseMessage GetSurgery(int surgeryId, int providerId, int locationId, string bundleFlag = null)
        {
            var schedules = DataAccess.SqlHelper.GetSurgery(surgeryId, providerId, locationId, bundleFlag);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetCase")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Surgery))]
        [Route("api/Surgery/case")]
        public HttpResponseMessage GetCase(int caseId, int providerId, int locationId)
        {
            var schedules = DataAccess.SqlHelper.GetCase(caseId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetByUserId")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/cases")]
        public HttpResponseMessage GetSurgeryScheduleByUser(int userId, int providerId, int locationId)
        {
            var surgeries = DataAccess.SqlHelper.GetScheduledSurgeries(userId, providerId, locationId);
            var open = DataAccess.SqlHelper.GetOpenSurgeries(userId, providerId, locationId);

            surgeries.AddRange(open);

            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        [SwaggerOperation("GetSurgeryRoomSchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/RoomSchedule")]
        public HttpResponseMessage GetRoomSurgerySchedule(int roomId, int providerId, int locationId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryRoomSchedule(roomId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetAlerts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/alerts")]
        public HttpResponseMessage GetSurgeryAlerts(int surgeryId, int providerId, int locationId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryAlerts(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetDelays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/delays")]
        public HttpResponseMessage GetSurgeryDelays(int surgeryId, int providerId, int locationId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryDelays(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetSurgeryUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryUser>))]
        [Route("api/Surgery/users")]
        public HttpResponseMessage GetSurgeryUsers(int caseId, int providerId, int locationId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryUsers(caseId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetVendorReps")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryVendorRep>))]
        [Route("api/Surgery/VendorReps")]
        public HttpResponseMessage GetSurgeryVendorReps(int surgeryId, int locationId, int providerId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryVendorReps(surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}