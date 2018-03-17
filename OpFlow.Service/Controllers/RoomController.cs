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

            var setups = DataAccess.SqlHelper.GetRoomSetups(roomSetupId, user.ProviderID, user.LocationID);

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
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public HttpResponseMessage Post([FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            var roomSetupId = DataAccess.SqlHelper.CreateRoomSetup(roomSetup, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomSetupId);
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Put(int id, [FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateRoomSetup(id, user.ProviderID, user.LocationID, roomSetup);

            return Request.CreateResponse(HttpStatusCode.OK, id);
        }
    }
}