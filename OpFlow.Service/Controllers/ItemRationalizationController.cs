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
        [SwaggerOperation("GetSummary")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalization>))]
        [Route("summary")]
        public async Task<HttpResponseMessage> GetSummary()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var specialties = await sqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var surgeons = await sqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetDisposableAudits(user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetDisposableCounts(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Specialties = specialties,
                Surgeons = surgeons,
                Audits = audits,
                Counts = counts
            });
        }

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
        [SwaggerOperation("SearchAuditCase")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemRationalizationCase>))]
        [Route("searchAudits")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchAuditCase(int? specialtyId, string itemName, int? surgeonId, int? cardId, DateTime beginDate, DateTime endDate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var items = await sqlHelper.GetItemAuditCases(specialtyId, itemName, surgeonId, cardId, beginDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("PostItemAudits")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("audits")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostItemAudits([FromBody] ItemAuditPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            await sqlHelper.UpdateDisposableAudit(post.Surgeries, post.Target, user.ProviderID, user.LocationID);

            List<TraySurgeryAudit> result;
            if (post.Target == "A")
                result = await sqlHelper.GetDisposableAudits(user.ProviderID, user.LocationID);
            else
                result = await sqlHelper.GetDisposableCounts(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
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

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}