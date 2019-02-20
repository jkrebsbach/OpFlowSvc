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
    public class ItemController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        public async Task<HttpResponseMessage> Get(string itemType = null, int? trayId = null, bool? countNeeded = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var items = await DataAccess.SqlHelper.GetItems(itemType, trayId, countNeeded, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetTrayItems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemTray>))]
        [Route("api/item/trayItems")]
        public async Task<HttpResponseMessage> GetTrayItems(int trayId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var items = await DataAccess.SqlHelper.GetTrayItems(trayId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/values/5
        [SwaggerOperation("GetVendorTrayHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemTray>))]
        [Route("api/item/trayHistory")]
        public async Task<HttpResponseMessage> GetVendorTrayHistory(int? specialtyId = null, int? userId = null, int? cardId = null,
            DateTime? beginDate = null, DateTime? endDate = null, int? itemId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var items = await DataAccess.SqlHelper.GetTrayItems(itemId ?? 0, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }
    }
}