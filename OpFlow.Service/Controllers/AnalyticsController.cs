using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Reporting.WebForms;
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPost]
        [Route("instrumentUsage")]
        public async Task<HttpResponseMessage> InstrumentUsageReport([FromBody] InstrumentUsagePost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CategoryID == null &&
                post.ProcedureID == null &&
                (post.Cpt == null || !post.Cpt.Any()) &&
                post.TrayID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, 0);
            }

            var analytics = await sqlHelper.GetInstrumentUsageReportData(post.SpecialtyID, post.SurgeonID, post.CategoryID, post.ProcedureID, post.Cpt, post.TrayID, user.ProviderID, user.LocationID);
            
            var parameters = new []
            {
                new ReportParameter("Group", post.Group)
            };
            var result = ReportHelper.GetReport("InstrumentUsage", analytics, parameters);

            var pngResult = ImageHelper.CreateWebImage(result);

            return Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(pngResult)
                });
        }

        // GET api/values/5
        [SwaggerOperation("ConcordanceReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPost]
        [Route("concordanceReport")]
        public async Task<HttpResponseMessage> ConcordanceReport([FromBody] InstrumentUsagePost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, 0);
            }

            var analytics = await sqlHelper.GetConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, post.TrayID, user.ProviderID, user.LocationID);

            var result = ReportHelper.GetReport("ConcordanceReport", analytics);

            var pngResult = ImageHelper.CreateWebImage(result);

            return Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(pngResult)
                });
        }

        // GET api/values/5
        [SwaggerOperation("TrayRationalizationReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPost]
        [Route("trayRationalization")]
        public async Task<HttpResponseMessage> TrayRationalizationReport([FromBody] TrayRationalizationReportPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, user.ProviderID, user.LocationID);
            /*
            switch (post.Order)
            {
                case "instrument_nbr":
                    analytics = analytics.OrderByDescending(a => a.InstrumentCount).ToList();
                    break;
                case "instrument_avg":
                    analytics = analytics.OrderByDescending(a => a.UsageQuantity).ToList();
                    break;
                case "tray_open":
                    analytics = analytics.OrderByDescending(a => a.TrayOpened).ToList();
                    break;
                case "tray_name":
                    analytics = analytics.OrderBy(a => a.TrayName).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.CaseCount).ToList();
                    break;
            }*/

            var payloadBytes = ReportHelper.GetReport("TrayRationalization", analytics);

            var pngResult = ImageHelper.CreateWebImage(payloadBytes);

            var result = Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(pngResult)
                });

            return result;
        }

        // GET api/values/5
        [SwaggerOperation("TrayRationalizationReportImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPost]
        [Route("trayRationalizationImage")]
        public async Task<HttpResponseMessage> TrayRationalizationReportImage([FromBody] TrayRationalizationReportPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, user.ProviderID, user.LocationID);
            /*
            switch (post.Order)
            {
                case "instrument_nbr":
                    analytics = analytics.OrderByDescending(a => a.InstrumentCount).ToList();
                    break;
                case "instrument_avg":
                    analytics = analytics.OrderByDescending(a => a.UsageQuantity).ToList();
                    break;
                case "tray_open":
                    analytics = analytics.OrderByDescending(a => a.TrayOpened).ToList();
                    break;
                case "tray_name":
                    analytics = analytics.OrderBy(a => a.TrayName).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.CaseCount).ToList();
                    break;
            }*/

            var payloadBytes = ReportHelper.GetReport("TrayRationalization", analytics);

            var result = Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(payloadBytes)
                });

            return result;
        }

        // GET api/values/5
        [SwaggerOperation("CountSummaryReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpPost]
        [Route("countSummary")]
        public async Task<HttpResponseMessage> CountSummaryReport([FromBody] CountSummaryReportPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, post.SurgeonId, post.CardId, post.RoomGroupId, user.ProviderID, user.LocationID);

            /*switch (post.Order)
            {
                case "specialty":
                    analytics = analytics.OrderBy(a => a.Specialty).ToList();
                    break;
                case "card":
                    analytics = analytics.OrderBy(a => a.Card).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.TrayCount).ToList();
                    break;
            }*/

            var parameters = new[]
            {
                new ReportParameter("Group", "t")
            };
            var result = ReportHelper.GetReport("CountSummary", analytics, parameters);

            var pngResult = ImageHelper.CreateWebImage(result);

            return Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(pngResult)
                });
        }

        // GET api/values/5
        [SwaggerOperation("CountSummaryByCardReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpPost]
        [Route("countSummaryByCard")]
        public async Task<HttpResponseMessage> CountSummaryByCardReport([FromBody] CountSummaryReportPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var analytics = await sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, post.SurgeonId, post.CardId, post.RoomGroupId, user.ProviderID, user.LocationID);

            /*            switch (post.Order)
            {
                case "specialty":
                    analytics = analytics.OrderBy(a => a.Specialty).ToList();
                    break;
                case "card":
                    analytics = analytics.OrderBy(a => a.Card).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.TrayCount).ToList();
                    break;
            }*/
            var parameters = new[]
            {
                new ReportParameter("Group", "c")
            };
            var result = ReportHelper.GetReport("CountSummary", analytics, parameters);

            var pngResult = ImageHelper.CreateWebImage(result);

            return Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(pngResult)
                });
        }
    }
}
