using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class RoomController : ApiController
    {
        // GET api/values
        [SwaggerOperation("GetByLocationId")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Room>))]
        public IEnumerable<Room> Get(int? roomId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var rooms = DataAccess.SqlHelper.GetRooms(user.LocationID);
            if (roomId.HasValue)
                rooms = rooms.Where(r => r.RoomID == roomId).ToList();

            return rooms;
        }
        // GET api/values
        [SwaggerOperation("GetTypes")]
        [Route("api/room/types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomType>))]
        public IEnumerable<RoomType> GetTypes(int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRoomTypes(user.LocationID);
        }
        // GET api/values
        [SwaggerOperation("GetSetups")]
        [Route("api/room/setups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomSetup>))]
        public IEnumerable<RoomSetup> GetSetups(int? roomSetupId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var setups = DataAccess.SqlHelper.GetRoomSetups(user.ProviderID, user.LocationID);

            if (roomSetupId != null)
                setups = setups.Where(s => s.RoomSetupID == roomSetupId).ToList();

            return setups;
        }

        // GET api/values
        [SwaggerOperation("GetPatientPositions")]
        [Route("api/room/patientPositions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<PatientPosition>))]
        public IEnumerable<PatientPosition> GetPatientPositions()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetPatientPositions(user.ProviderID, user.LocationID);
        }

        // POST api/values
        [SwaggerOperation("UpdateRoomSetupEquipment")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [HttpPost]
        [Route("api/surgery/updateRoomSetupEquipment", Name = "UpdateRoomSetupEquipment")]
        public async Task<HttpResponseMessage> UpdateRoomSetupEquipment([FromBody]RoomSetupEquipment equipment)
        {
            return Request.CreateResponse(HttpStatusCode.Created, 0);
        }

        // POST api/values
        [SwaggerOperation("UpdateRoomSetupItem")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [HttpPost]
        [Route("api/room/updateRoomSetupItem", Name = "UpdateRoomSetupItem")]
        public async Task<HttpResponseMessage> UpdateRoomSetupItem([FromBody]RoomSetupItem item)
        {
            return Request.CreateResponse(HttpStatusCode.Created, 0);
        }

        // DELETE api/values/5
        [SwaggerOperation("DeleteRoomSetupEquipment")]
        [Route("api/room/deleteRoomSetupEquipment", Name = "DeleteRoomSetupEquipment")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteRoomSetupEquipment(int roomSetupEquipmentId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteRoomSetupEquipment(roomSetupEquipmentId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // DELETE api/values/5
        [SwaggerOperation("DeleteRoomSetupItem")]
        [Route("api/room/deleteRoomSetupItem", Name = "DeleteRoomSetupItem")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage DeleteRoomSetupItem(int roomSetupItemId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteRoomSetupItem(roomSetupItemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }
    }
}