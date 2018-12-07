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
    public class AnalyticsController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetAnalytics")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(AnalyticsSummary))]
        [Route("api/analytics")]
        public async Task<HttpResponseMessage> GetAnalytics(int? tableId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var analyticsSummary = await SqlHelper.GetAnalytics(user.ProviderID, user.LocationID, tableId);
            
            return Request.CreateResponse(HttpStatusCode.OK, analyticsSummary);
        }
    }
}