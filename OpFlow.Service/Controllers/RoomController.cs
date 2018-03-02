using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        public IEnumerable<Room> Get(int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRooms(user.LocationID);
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
        public IEnumerable<RoomSetup> GetSetups(int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            return DataAccess.SqlHelper.GetRoomSetups(user.ProviderID, user.LocationID);
        }
    }
}