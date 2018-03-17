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
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(PatientSurgery))]
        public async Task<HttpResponseMessage> GetSurgery(int surgeryId, int? providerId = null, int? locationId = null, string bundleFlag = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var patientSurgery = DataAccess.SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID, bundleFlag);
            patientSurgery.Patient =
                await DataAccess.SecureSqlHelper.GetPatient(patientSurgery.PatientID, user.DatabaseName);

            return Request.CreateResponse(HttpStatusCode.OK, patientSurgery);
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
        [SwaggerOperation("SearchCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        [Route("api/Surgery/searchCases")]
        public HttpResponseMessage GetCases(string caseNbr = null, int? surgeonUserId = null, int? roomId = null, 
            DateTime? begDate = null, DateTime? endDate = null, int? providerId = null, int? locationId = null)
        {
            if (caseNbr == null && surgeonUserId == null && roomId == null)
                return Request.CreateResponse(HttpStatusCode.Ambiguous);

            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.SearchCases(caseNbr, surgeonUserId, roomId,
                begDate, endDate, user.ProviderID, user.LocationID);

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
        public HttpResponseMessage GetSurgeonCases(int surgeonUserId, DateTime begDate, DateTime endDate, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeonCases(surgeonUserId, begDate, endDate, user.ProviderID, user.LocationID);

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
        [SwaggerOperation("GetSchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySchedule>))]
        [Route("api/Surgery/cases")]
        public HttpResponseMessage GetSurgerySchedule(DateTime? scheduleDate = null, int? roomId = null, int ? userId = null, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var surgeries = DataAccess.SqlHelper.GetScheduledSurgeries(user.UserID, user.ProviderID, user.LocationID, scheduleDate, roomId);
            var open = DataAccess.SqlHelper.GetOpenSurgeries(user.UserID, user.ProviderID, user.LocationID, scheduleDate, roomId);

            surgeries.AddRange(open);

            foreach (var surgery in surgeries)
            {
                surgery.SurgeryUsers =
                    DataAccess.SqlHelper.GetSurgeryUsers(surgery.CaseID, user.ProviderID, user.LocationID);
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        [SwaggerOperation("GetSurgeryRoomSchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/RoomSchedule")]
        public HttpResponseMessage GetRoomSurgerySchedule(int roomId, DateTime? scheduleDate = null, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var schedules = DataAccess.SqlHelper.GetSurgeryRoomSchedule(roomId, scheduleDate, user.ProviderID, user.LocationID);

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

        [SwaggerOperation("GetUtilizationCounts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SurgeryUtilizationCount))]
        [Route("api/Surgery/UtilizationCounts")]
        public HttpResponseMessage GetSurgeryUtilizationCounts(int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();

            var surgeryUtilization = new SurgeryUtilizationCount()
            {
                SurgeryItemCounts = DataAccess.SqlHelper.GetSurgeryItemCounts(surgeryId, user.ProviderID, user.LocationID),
                SurgeryInstrumentCounts =
                    DataAccess.SqlHelper.GetSurgeryInstrumentCounts(surgeryId, user.ProviderID, user.LocationID)
            };

            return Request.CreateResponse(HttpStatusCode.OK, surgeryUtilization);
        }

        // POST api/values
        [SwaggerOperation("AssignCard")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignCard", Name = "AssignCard")]
        public async Task<IHttpActionResult> AssignToCard(int surgeryId, int cardId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignCardToCase(cardId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignRoomCase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignRoom", Name = "AssignRoomCase")]
        public async Task<IHttpActionResult> AssignRoomToCase(int surgeryId, int roomId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignRoomToCase(roomId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignRoomSetup", Name = "AssignRoomSetupCase")]
        public async Task<IHttpActionResult> AssignRoomSetupToCase(int surgeryId, int roomSetupId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignRoomSetupToCase(roomSetupId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignFlow", Name = "AssignFlowCase")]
        public async Task<IHttpActionResult> AssignToFlowCase(int surgeryId, int flowId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AssignFlowToCase(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]SurgeryPost surgery)
        {
            var user = CacheUtil.GetUserSecurity();
            var patientId = await DataAccess.SecureSqlHelper.CreatePatient(surgery.PtAcctNbr, surgery.PtInitials,
                surgery.PtDOB, surgery.PtGender, surgery.PtFirstName, surgery.PtLastName, surgery.PtBMI, user.DatabaseName);

            var caseId = DataAccess.SqlHelper.CreateCase(patientId, user.UserID, surgery.SpecialtyID, user.ProviderID,
                user.LocationID, surgery.CaseNbr);

            var cardFlowRoom = surgery.BundleID.HasValue ? 
                DataAccess.SqlHelper.GetBundleDefaultCardFlowRoom(surgery.BundleID.Value).FirstOrDefault() : 
                DataAccess.SqlHelper.GetProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, surgery.CptCode).FirstOrDefault();

            var surgeryId = DataAccess.SqlHelper.CreateSurgery(surgery, user.ProviderID, user.LocationID, patientId, caseId, 
                cardFlowRoom?.ProcedureID, cardFlowRoom?.CardID, cardFlowRoom?.TemplateFlowID, cardFlowRoom?.TemplateRoomID);

            return Request.CreateResponse(HttpStatusCode.Created, surgeryId);
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryCounts")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/updateCounts", Name = "UpdateSurgeryCounts")]
        public async Task<HttpResponseMessage> UpdateSurgeryCounts([FromBody]SurgeryCountPost counts)
        {
             return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("StartSurgery")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/start", Name = "StartSurgery")]
        public async Task<HttpResponseMessage> StartSurgery(int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();
            var success = DataAccess.SqlHelper.StartSurgery(surgeryId, user.ProviderID, user.LocationID);

            success = DataAccess.SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, success);
        }

        // POST api/values
        [SwaggerOperation("MoveNextStep")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        [Route("api/surgery/nextStep", Name = "MoveNextStep")]
        public async Task<HttpResponseMessage> MoveNextStep(int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();
            var success = DataAccess.SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.Created, success);
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