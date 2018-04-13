using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.Models;
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
        [SwaggerOperation("GetDelayReasons")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryDelayReason>))]
        [Route("api/Surgery/delayReasons")]
        public HttpResponseMessage GetSurgeryDelayReasons()
        {
            var user = CacheUtil.GetUserSecurity();

            var reasons = DataAccess.SqlHelper.GetSurgeryDelayReasons(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, reasons);
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

        // GET api/values/5
        [SwaggerOperation("GetSurgeryCardList")]
        [Route("api/surgery/cards")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCard>))]
        public HttpResponseMessage GetSurgeryCardList(int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetSurgeryCardList(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItemCounts")]
        [Route("api/surgery/cardItemCounts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardItemCountResult))]
        public HttpResponseMessage GetSurgeryCardItemCounts(int surgeryId)
        {
            var user = CacheUtil.GetUserSecurity();

            var itemCounts = DataAccess.SqlHelper.GetSurgeryCardItemCounts(surgeryId, user.ProviderID, user.LocationID)
                .GroupBy(ic => ic.ItemType);

            var result = new CardItemCountResult();

            foreach (var countType in itemCounts)
            {
                if (countType.Key == "SUPPLY")
                    result.Supplies = itemCounts.First(ic => ic.Key == "SUPPLY").ToList();
                else if (countType.Key == "INSTRUMENT")
                    result.Instruments = itemCounts.First(ic => ic.Key == "INSTRUMENT").ToList();
                else
                {
                    var trayItems = countType.ToList();

                    result.Trays[trayItems.FirstOrDefault()?.TrayID ?? 0] = countType.ToList();

                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }



        // GET api/values/5
        [SwaggerOperation("GetDebriefScreen")]
        [Route("api/surgery/debriefScreen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DebriefResult))]
        public HttpResponseMessage GetDebriefScreen(int flowId, int? surgeryId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var categories = DataAccess.SqlHelper.GetSmartPhraseCategories(user.ProviderID, user.LocationID);
            var flowPhrases = DataAccess.SqlHelper.GetFlowPhrases(flowId, user.ProviderID, user.LocationID);
            var flowFeedback = DataAccess.SqlHelper.GetFlowFeedback(flowId, user.ProviderID, user.LocationID);
            var surgeonNotes = DataAccess.SqlHelper.GetSurgeonNotes(flowId, user.ProviderID, user.LocationID);
            var flowSteps = DataAccess.SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID);
            var messages =
                DataAccess.SqlHelper.GetMessaging(user.UserID, surgeryId, null, null, user.ProviderID, user.LocationID);

            var flowImages = DataAccess.SqlHelper.GetFlowImages(flowId, user.ProviderID, user.LocationID);

            foreach (var flowPhrase in flowPhrases)
            {
                var category = categories.FirstOrDefault(c => c.CategoryID == flowPhrase.CategoryID);

                category?.FlowPhrases.Add(flowPhrase);
            }

            var result = new DebriefResult()
            {
                Categories = categories,
                FlowFeedback = flowFeedback,
                SurgeonNotes = surgeonNotes,
                FlowSteps = flowSteps,
                Messages = messages,
                FlowImages = flowImages
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("UpdateDebrief")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/debrief", Name = "UpdateDebrief")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateDebrief(int surgeryId, int flowId, [FromBody]DebriefUpdatePost debriefUpdate)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateDebrief(flowId, debriefUpdate?.SelectedPhrases, user.ProviderID, user.LocationID);
            DataAccess.SqlHelper.UpdateCaseNotes(surgeryId, debriefUpdate?.CaseNotes, user.ProviderID, user.LocationID);

            return Ok();
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
        public async Task<HttpResponseMessage> UpdateSurgeryCounts(int surgeryId, [FromBody]SurgeryCountPost countModel)
        {
            var user = CacheUtil.GetUserSecurity();

            foreach (var count in countModel.ItemCounts)
            {
                DataAccess.SqlHelper.UpdateSurgeryCount(surgeryId, count.ItemID, count.Pass1, count.Pass2, count.Pass3, count.Usage, user.ProviderID, user.LocationID);
            }
            foreach (var count in countModel.InstrumentCounts)
            {
                DataAccess.SqlHelper.UpdateSurgeryInstrumentCount(surgeryId, count.ItemID, count.Pass1, count.Pass2, count.Pass3, count.Usage, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("StartSurgery")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/start", Name = "StartSurgery")]
        public async Task<HttpResponseMessage> StartSurgery(int surgeryId, DateTime startTime)
        {
            var user = CacheUtil.GetUserSecurity();
            var success = DataAccess.SqlHelper.StartSurgery(surgeryId, user.ProviderID, user.LocationID, startTime);

            var flowStep = DataAccess.SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID, startTime);
            FlowStepNotifications(flowStep);

            return Request.CreateResponse(HttpStatusCode.OK, success);
        }

        // POST api/values
        [SwaggerOperation("MoveNextStep")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        [Route("api/surgery/nextStep", Name = "MoveNextStep")]
        public async Task<HttpResponseMessage> MoveNextStep(int surgeryId, DateTime stepTime)
        {
            var user = CacheUtil.GetUserSecurity();

            var flowStep = DataAccess.SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID, stepTime);
            FlowStepNotifications(flowStep);

            return Request.CreateResponse(HttpStatusCode.Created, flowStep);
        }

        // POST api/values
        [SwaggerOperation("FinishSurgery")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/finish", Name = "FinishSurgery")]
        public async Task<HttpResponseMessage> FinishSurgery(int surgeryId, DateTime finishTime)
        {
            var user = CacheUtil.GetUserSecurity();

            var flowStep = DataAccess.SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID, finishTime);
            FlowStepNotifications(flowStep);

            return Request.CreateResponse(HttpStatusCode.OK, flowStep);
        }

        private static void FlowStepNotifications(FlowStep flowStep)
        {
            var user = CacheUtil.GetUserSecurity();
            if (flowStep == null)
                return;

            var notifications = DataAccess.SqlHelper.GetFlowNotifications(flowStep.FlowID, null, user.ProviderID, user.LocationID);

            var startNotification = notifications.FirstOrDefault(n => n.StepID == flowStep.StepID && n.NotificationType == 1);
            var endNotification = notifications.FirstOrDefault(n => n.StepID == flowStep.StepID - 1 && n.NotificationType == 2);

            SendNotification(startNotification);
            SendNotification(endNotification);
        }

        private static void SendNotification(FlowNotification flowNotification)
        {
            var user = CacheUtil.GetUserSecurity();
            if (flowNotification == null)
                return;

            NotificationSystem.NotifyUser(flowNotification.CellPhone, flowNotification.FlowMessage);

            if (flowNotification.MessagingUserID != null)
            {
                DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID, null, flowNotification.MessagingUserID, flowNotification.FlowMessage);

            }
        }

        // POST api/values
        [SwaggerOperation("ToggleDelay")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        [HttpPost]
        [Route("api/surgery/toggleDelay", Name = "ToggleDelay")]
        public async Task<HttpResponseMessage> ToggleSurgeryDelay(int surgeryId, [FromBody]SurgeryDelayPost surgeryDelay)
        {
            if (surgeryDelay.StartTime == null && surgeryDelay.EndTime == null)
                return Request.CreateResponse(HttpStatusCode.Ambiguous);

            var user = CacheUtil.GetUserSecurity();
            var success = DataAccess.SqlHelper.SurgeryToggleDelay(surgeryId, user.ProviderID, user.LocationID, 
                surgeryDelay.StartTime, surgeryDelay.EndTime, surgeryDelay.DelayReasonID);

            return Request.CreateResponse(HttpStatusCode.Created, success);
        }

        // POST api/values
        [SwaggerOperation("EditSurgeryProperties")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        [Route("api/surgery/editProperties", Name = "EditSurgeryProperties")]
        public async Task<HttpResponseMessage> EditSurgeryProperties(int surgeryId, [FromBody]SurgeryEditPost surgeryEditPost)
        {
            var user = CacheUtil.GetUserSecurity();
            var success = DataAccess.SqlHelper.SurgeryEditProperties(surgeryId,
                surgeryEditPost.RoomID, surgeryEditPost.ScheduleDateTime, user.ProviderID, user.LocationID);

            if (surgeryEditPost.NotificationUser.HasValue)
            {
                var message = $"Surgery schedule modified - please review schedule";

                var notificationUser = DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, surgeryEditPost.NotificationUser.Value);
                if (notificationUser?.CellPhone != null)
                    NotificationSystem.NotifyUser(notificationUser.CellPhone, message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, success);
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