using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<Room> Get(int locationId)
        {
            return DataAccess.SqlHelper.GetRooms(locationId);
        }
    }
}