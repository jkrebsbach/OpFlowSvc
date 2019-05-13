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
            var procedures = await sqlHelper.GetProcedures(null, user.ProviderID, user.LocationID);
            var trays = await sqlHelper.GetItems("TRAY", null, null, user.ProviderID, user.LocationID);
            var categories = await sqlHelper.GetProposedTrayInstrumentCategories(user.ProviderID, user.LocationID);
            var roomGroups = await sqlHelper.GetRoomGroups(user.ProviderID, user.LocationID);
            var cpts = await sqlHelper.GetKnownCPTCodes(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Reports = reports,
                Parameters = new
                {
                    Specialties = specialties,
                    Surgeons = surgeons,
                    Trays = trays,   
                    Procedures = procedures,
                    RoomGroups = roomGroups,
                    Categories = categories,
                    CPTs = cpts
                }
            });
        }

        // GET api/values/5
        [SwaggerOperation("InstrumentUsageReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsInstrumentUsage>))]
        [HttpPost]
        [Route("instrumentUsage")]
        public async Task<HttpResponseMessage> InstrumentUsageReport([FromBody] InstrumentUsagePost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetInstrumentUsageReport(post.SpecialtyID, post.SurgeonID, post.CategoryID, post.ProcedureID, post.Cpt, post.TrayID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Instruments = analytics.Select(t => t.Instrument),
                QtyOpen = analytics.Select(t => t.QtyOpen)
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
                UsageQuantity = analytics.Select(t => t.UsageQuantity),
                TrayOpened = analytics.Select(t => t.TrayOpened)
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetCountSummary")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpGet]
        [Route("countSummary")]
        public async Task<HttpResponseMessage> GetCountSummary(int? specialtyId, int? surgeonId, int? cardId, int? roomGroupId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsCountSummary(specialtyId, surgeonId, cardId, roomGroupId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Specialties = analytics.Select(t => t.Specialty),
                TrayCount = analytics.Select(t => t.TrayCount),
                CardCount = analytics.Select(t => t.CardCount)
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetCountSummaryByCard")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpGet]
        [Route("countSummaryByCard")]
        public async Task<HttpResponseMessage> GetCountSummaryByCard(int? specialtyId, int? surgeonId, int? cardId, int? roomGroupId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsCountSummaryByCard(specialtyId, surgeonId, cardId, roomGroupId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Cards = analytics.Select(t => t.Card),
                TrayCount = analytics.Select(t => t.TrayCount),
                CardCount = analytics.Select(t => t.CardCount)
            });
        }
    }
}
