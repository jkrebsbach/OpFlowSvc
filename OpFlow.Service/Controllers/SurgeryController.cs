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
        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int id)
        {
            var schedules = DataAccess.SqlHelper.GetSurgery(id);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/values/5
        [SwaggerOperation("GetByUserId")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetSurgeryByUser(int userId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeries(userId);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetSurgeryUsers")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/Surgery/{id}/users")]
        public HttpResponseMessage GetSurgeryUsers(int id)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryUsers(id);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetSurgeryRoomSchedule")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/Surgery/RoomSchedule")]
        public HttpResponseMessage GetRoomSurgerySchedule(int roomId, int providerId, int locationId)
        {
            var schedules = DataAccess.SqlHelper.GetSurgeryRoomSchedule(roomId, providerId, locationId);

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