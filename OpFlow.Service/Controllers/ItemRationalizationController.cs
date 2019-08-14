using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/itemRationalization")]
    public class ItemRationalizationController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetItemRationalizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalization>))]
        [Route("item")]
        public async Task<HttpResponseMessage> GetItemRationalizations(int? specialtyId, int? surgeonId, decimal? minCost, decimal? maxCost)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var items = await sqlHelper.GetItemRationalization(specialtyId, surgeonId, minCost, maxCost, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("SearchItemRationalizations")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalization>))]
        [Route("search")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchItemRationalizations(string itemName = null, decimal? minCost = null, decimal? maxCost = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var items = await sqlHelper.SearchItems(itemName, minCost, maxCost, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("PutCountNeeded")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("countNeeded")]
        public async Task<HttpResponseMessage> PutCountNeeded(int itemId, bool countNeeded)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.UpdateItemCountNeeded(itemId, countNeeded, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, itemId);
        }
    }
}