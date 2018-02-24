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
    public class SurgeryController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetSurgery")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(Surgery))]
        public HttpResponseMessage GetSurgery(int surgeryId, int? providerId = null, int? locationId = null, string bundleFlag = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID, bundleFlag);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetCase")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Surgery))]
        [Route("api/Surgery/case")]
        public HttpResponseMessage GetCase(int caseId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetCase(caseId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchCaseNbr")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("api/Surgery/searchCaseNbr")]
        public HttpResponseMessage GetCaseNbr(string caseNbr, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetCaseNbr(caseNbr, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchSurgeonCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("api/Surgery/searchSurgeonCases")]
        public HttpResponseMessage GetSurgeonCases(DateTime begDate, DateTime endDate, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeonCases(begDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchRoomCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("api/Surgery/searchRoomCases")]
        public HttpResponseMessage GetRoomCases(int roomId, DateTime begDate, DateTime endDate, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetRoomCases(roomId, begDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetByUserId")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/cases")]
        public HttpResponseMessage GetSurgeryScheduleByUser(int? userId = null, int? providerId = null, int? locationId = null, DateTime? scheduleDate = null, int? roomId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var surgeries = DataAccess.SqlHelper.GetScheduledSurgeries(userId, user.ProviderID, user.LocationID, scheduleDate, roomId);
            var open = DataAccess.SqlHelper.GetOpenSurgeries(userId, user.ProviderID, user.LocationID, scheduleDate, roomId);

            surgeries.AddRange(open);

            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        [SwaggerOperation("GetSurgeryRoomSchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/RoomSchedule")]
        public HttpResponseMessage GetRoomSurgerySchedule(int roomId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeryRoomSchedule(roomId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetAlerts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/alerts")]
        public HttpResponseMessage GetSurgeryAlerts(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeryAlerts(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetDelays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/delays")]
        public HttpResponseMessage GetSurgeryDelays(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeryDelays(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetSurgeryUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryUser>))]
        [Route("api/Surgery/users")]
        public HttpResponseMessage GetSurgeryUsers(int caseId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeryUsers(caseId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetVendorReps")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryVendorRep>))]
        [Route("api/Surgery/VendorReps")]
        public HttpResponseMessage GetSurgeryVendorReps(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeryVendorReps(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<IHttpActionResult> Post([FromBody]SurgeryPost surgery)
        {
            DataAccess.SqlHelper.CreateSurgery(surgery);

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}