using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/analytics")]
    public class AnalyticsController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("GetReports")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetReports()
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var reports = await sqlHelper.GetPowerBIReports(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, reports);
        }
    }
}
