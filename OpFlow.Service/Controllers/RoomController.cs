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
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Room>))]
        [HttpGet]
        public IEnumerable<Room> Get(int? roomId = null, int? roomGroupId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var rooms = DataAccess.SqlHelper.GetRooms(user.LocationID);
            if (roomId.HasValue)
                rooms = rooms.Where(r => r.RoomID == roomId).ToList();

            if (roomGroupId.HasValue)
                rooms = rooms.Where(r => r.RoomGroupID == roomGroupId).ToList();

            return rooms;
        }

        // GET api/values
        [SwaggerOperation("GetTypes")]
        [Route("api/room/types")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomType>))]
        public IEnumerable<RoomType> GetTypes()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRoomTypes(user.LocationID);
        }

        // GET api/values
        [SwaggerOperation("GetGroups")]
        [Route("api/room/groups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomGroup>))]
        public IEnumerable<RoomGroup> GetGroups()
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRoomGroups(user.ProviderID, user.LocationID);
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
        [SwaggerOperation("GetSetupDetail")]
        [Route("api/room/setupDetail")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(RoomSetupDetail))]
        public RoomSetupDetail GetSetupDetail(int? roomSetupId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var setups = DataAccess.SqlHelper.GetRoomSetups(roomSetupId, user.ProviderID, user.LocationID);

            if (roomSetupId != null)
                setups = setups.Where(s => s.RoomSetupID == roomSetupId).ToList();

            var roomTypes = DataAccess.SqlHelper.GetRoomTypes(user.LocationID);
            var patientPositions = DataAccess.SqlHelper.GetPatientPositions(user.ProviderID, user.LocationID);

            var equipment = DataAccess.SqlHelper.GetItems("EQUIPMENT", null, null, user.ProviderID, user.LocationID);
            var instruments = DataAccess.SqlHelper.GetItems("INSTRUMENT", null, null, user.ProviderID, user.LocationID);

            var result = new RoomSetupDetail()
            {
                RoomSetups = setups,
                RoomTypes = roomTypes,
                PatientPositions = patientPositions,
                EquipmentItems = equipment,
                InstrumentItems = instruments
            };

            return result;
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

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Update")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        public HttpResponseMessage PutRoom(int roomId, [FromBody]RoomPost room)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateRoom(roomId, room.Description, room.RoomTypeID, room.RoomGroupID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Create")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public HttpResponseMessage PostRoom([FromBody]RoomPost room)
        {
            var user = CacheUtil.GetUserSecurity();

            var roomId = DataAccess.SqlHelper.InsertRoom(room.Description, room.RoomTypeID, room.RoomGroupID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Delete")]
        [Route("api/room")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public HttpResponseMessage DeleteRoom(int roomId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteRoom(roomId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomId);
        }

        // POST api/roomSetup/values
        [SwaggerOperation("CreateSetup")]
        [Route("api/room/roomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        public HttpResponseMessage Post([FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            var roomSetupId = DataAccess.SqlHelper.CreateRoomSetup(roomSetup, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomSetupId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("UpdateSetup")]
        [Route("api/room/roomSetup/{roomsetupId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        public HttpResponseMessage PutRoomSetup(int roomsetupId, [FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateRoomSetup(roomsetupId, user.ProviderID, user.LocationID, roomSetup);

            return Request.CreateResponse(HttpStatusCode.OK, roomsetupId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("DeleteSetup")]
        [Route("api/room/roomSetup/{roomsetupId}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteSetup(int roomsetupId, [FromBody]RoomSetup roomSetup)
        {
            var user = CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.DeleteRoomSetup(roomsetupId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, roomsetupId);
        }
    }
}