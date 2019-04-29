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
            var specialties = await sqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var surgeons = await sqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
            var trays = await sqlHelper.GetItems("TRAY", null, null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Reports = reports,
                Parameters = new
                {
                    Specialties = specialties,
                    Surgeons = surgeons,
                    Trays = trays,   
                }
            });
        }


        // GET api/values/5
        [SwaggerOperation("GetTrayRationalization")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpGet]
        [Route("trayRationalization")]
        public async Task<HttpResponseMessage> GetTrayRationalization(int? specialtyId, int? surgeonId, int? trayId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsTrayRationalization(specialtyId, surgeonId, trayId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Trays = analytics.Select(t => t.TrayName),
                CaseCount = analytics.Select(t => t.CaseCount),
                InstrumentCount = analytics.Select(t => t.InstrumentCount),
                UsageQuantity = analytics.Select(t => t.UsageQuantity)
            });
        }
    }
}
