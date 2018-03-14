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
    public class ItemController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        public HttpResponseMessage Get(string itemType = null, int? trayId = null, bool? countNeeded = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var items = DataAccess.SqlHelper.GetItems(itemType, trayId, countNeeded, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemTray>))]
        [Route("api/item/trays")]
        public HttpResponseMessage GetTrays()
        {
            var user = CacheUtil.GetUserSecurity();

            var items = DataAccess.SqlHelper.GetItemTrays(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }
    }
}