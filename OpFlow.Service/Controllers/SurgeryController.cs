using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.DataAccess;
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
        public async Task<HttpResponseMessage> GetSurgery(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            var patientSurgery = await SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);
            patientSurgery.Patient =
                await SecureSqlHelper.GetPatient(patientSurgery.PatientID,
                user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID, 
                user.SecureDatabaseName);

            return Request.CreateResponse(HttpStatusCode.OK, patientSurgery);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetNewSurgerySetup")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NewSurgerySetup))]
        [Route("api/Surgery/newSurgerySetup")]
        public async Task<HttpResponseMessage> GetNewSurgerySetup()
        {
            var user = await CacheUtil.GetUserSecurity();

            var rooms = await SqlHelper.GetRooms(user.LocationID);
            var specialties = await SqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var lateralities = await SqlHelper.GetLateralities(user.ProviderID, user.LocationID);

            var result = new NewSurgerySetup()
            {
                Rooms = rooms,
                Specialties = specialties,
                Lateralities = lateralities
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetCase")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Surgery))]
        [Route("api/Surgery/case")]
        public async Task<HttpResponseMessage> GetCase(int caseId, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetCase(caseId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        [Route("api/Surgery/searchCases")]
        public async Task<HttpResponseMessage> GetCases(string caseNbr = null, int? surgeonUserId = null, int? userId = null, 
            int? roomGroupId = null, int? roomId = null, 
            int? bundleId = null, int? procedureId = null, int? specialtyId = null,
            DateTime? begDate = null, DateTime? endDate = null, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.SearchCases(userId, surgeonUserId, roomGroupId, roomId,
                bundleId, procedureId, specialtyId,
                begDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchCaseNbr")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("api/Surgery/searchCaseNbr")]
        public async Task<HttpResponseMessage> GetCaseNbr(string caseNbr, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetCaseNbr(caseNbr, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchSurgeonCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("api/Surgery/searchSurgeonCases")]
        public async Task<HttpResponseMessage> GetSurgeonCases(int surgeonUserId, DateTime begDate, DateTime endDate, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetSurgeonCases(surgeonUserId, begDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchRoomCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("api/Surgery/searchRoomCases")]
        public async Task<HttpResponseMessage> GetRoomCases(int roomId, DateTime begDate, DateTime endDate, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetRoomCases(roomId, begDate, endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetSchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySchedule>))]
        [Route("api/Surgery/cases")]
        public async Task<HttpResponseMessage> GetSurgerySchedule(DateTime? scheduleDate = null, int? roomId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            int? userId = null;
            if (roomId == null)
                userId = user.UserID;

            var surgeries = await SqlHelper.GetScheduledSurgeries(userId, user.ProviderID, user.LocationID, scheduleDate, roomId);
            
            foreach (var surgery in surgeries)
            {
                surgery.SurgeryUsers =
                    await SqlHelper.GetSurgeryUsers(surgery.CaseID, user.ProviderID, user.LocationID);
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetAlerts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("api/Surgery/alerts")]
        public async Task<HttpResponseMessage> GetSurgeryAlerts(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetSurgeryAlerts(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<BundleProcedure>))]
        [Route("api/surgery/procedures")]
        public async Task<HttpResponseMessage> GetSurgeryProcedures(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var procedures = await SqlHelper.GetSurgeryProcedures(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, procedures);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetDelayReasons")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryDelayReason>))]
        [Route("api/Surgery/delayReasons")]
        public async Task<HttpResponseMessage> GetSurgeryDelayReasons()
        {
            var user = await CacheUtil.GetUserSecurity();

            var reasons = await SqlHelper.GetSurgeryDelayReasons(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, reasons);
        }

        [SwaggerOperation("GetSurgeryUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryUser>))]
        [Route("api/Surgery/users")]
        public async Task<HttpResponseMessage> GetSurgeryUsers(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetSurgeryUsers(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetVendorReps")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryVendorRep>))]
        [Route("api/Surgery/VendorReps")]
        public async Task<HttpResponseMessage> GetSurgeryVendorReps(int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var schedules = await SqlHelper.GetSurgeryVendorReps(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/values/5
        [SwaggerOperation("GetSurgeryCardList")]
        [Route("api/surgery/cards")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCard>))]
        public async Task<HttpResponseMessage> GetSurgeryCardList(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetSurgeryCardList(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetSurgeryFlowList")]
        [Route("api/surgery/flows")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCard>))]
        public async Task<HttpResponseMessage> GetSurgeryFlowList(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetSurgeryFlowList(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItems")]
        [Route("api/surgery/carditems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCardItem>))]
        public async Task<HttpResponseMessage> GetCardItems(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetSurgeryCardItems(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItemCounts")]
        [Route("api/surgery/cardItemCounts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardItemCountResult))]
        public async Task<HttpResponseMessage> GetSurgeryCardItemCounts(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var cardItemCounts = await SqlHelper.GetSurgeryCardItemCounts(surgeryId, user.ProviderID, user.LocationID);
            var itemCounts = cardItemCounts.CardItemCounts.GroupBy(ic => new { ic.ItemType, ic.TrayName, ic.TrayID});

            var trayOpens = await SqlHelper.GetSurgeryTrayOpens(surgeryId, user.ProviderID, user.LocationID);

            var result = new CardItemCountResult()
            {
                Collections = new List<TrayCollection>(),
                Trays = new List<TrayUsage>()
            };

            foreach (var countType in itemCounts)
            {
                switch (countType.Key.ItemType)
                {
                    case "SUPPLY":
                        result.Supplies = countType.ToList();
                        break;
                    case "INSTRUMENT":
                        result.Instruments = countType.ToList();
                        break;
                    case "COLLECTION":
                        result.Collections.AddRange(countType.Select(c => new TrayCollection()
                        {
                            ItemID = c.ItemID,
                            ItemDescription = c.ItemDescription
                        }));
                        break;
                    case "TRAY":
                        result.Trays.Add(new TrayUsage()
                        {
                            TrayID = countType.Key.TrayID ?? 0,
                            TrayItems = countType.ToList()
                        });
                        break;
                    default:
                        break;
                }
            }

            foreach (var collection in result.Collections)
            {
                collection.Questions = cardItemCounts.TrayQuestions
                    .Where(c => c.CollectionItemID == collection.ItemID).ToList();

                collection.CollectionItems = cardItemCounts.TrayCollectionCounts
                    .Where(c => c.CollectionItemID == collection.ItemID).ToList();
            }

            foreach (var trayOpen in trayOpens)
            {
                var tray = result.Trays.FirstOrDefault(t => t.TrayID == trayOpen.TrayID);
                tray.TrayOpened = trayOpen.TrayOpened;
            }

            

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [SwaggerOperation("UpdateTrayOpen")]
        [Route("api/surgery/updateTrayOpen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        public async Task<HttpResponseMessage> UpdateSurgeryTrayOpen(int surgeryId, int trayId, bool trayOpened)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.UpdateSurgeryTrayOpens(surgeryId, user.ProviderID, user.LocationID, trayId, trayOpened);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }

        // GET api/values/5
        [SwaggerOperation("GetSearchScreen")]
        [Route("api/surgery/searchScreen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SearchScreen))]
        public async Task<HttpResponseMessage> GetSearchScreen()
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = new SearchScreen
            {
                Rooms = await SqlHelper.GetRooms(user.LocationID),
                RoomGroups = await SqlHelper.GetRoomGroups(user.ProviderID, user.LocationID),
                Users = await SqlHelper.SearchUsers(null, null, null, user.ProviderID, user.LocationID),
                Specialties = await SqlHelper.GetSpecialties(user.ProviderID, user.LocationID),
                Bundles = await SqlHelper.GetBundles(null, user.ProviderID, user.LocationID),
                Procedures = await SqlHelper.GetProcedures(null, user.ProviderID, user.LocationID)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetDebriefScreen")]
        [Route("api/surgery/debriefScreen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DebriefResult))]
        public async Task<HttpResponseMessage> GetDebriefScreen(int flowId, int? surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var flow = await SqlHelper.GetFlow(flowId, user.ProviderID, user.LocationID);
            var categories = await SqlHelper.GetSmartPhraseCategories(user.ProviderID, user.LocationID);
            var flowPhrases = await SqlHelper.GetFlowPhrases(flowId, user.ProviderID, user.LocationID);

            var smartPhrases = new List<SmartPhrase>();
            if (flow != null)
                smartPhrases = await SqlHelper.GetSmartPhrases(null, null, flow.OwnerUserID, user.ProviderID, user.LocationID);

            var flowFeedback = await SqlHelper.GetFlowFeedback(flowId, user.ProviderID, user.LocationID);
            var surgeonNotes = await SqlHelper.GetSurgeonNotes(flowId, user.ProviderID, user.LocationID);
            var flowSteps = await SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID);
            var messages =
                await SqlHelper.GetMessaging(user.UserID, surgeryId, null, null, user.ProviderID, user.LocationID);

            var flowImages = await SqlHelper.GetFlowImages(flowId, user.ProviderID, user.LocationID);

            var surgeryPhrases = new List<SurgeryPhrase>();
            var surgeryImages = new List<SurgeryImage>();
            
            if (surgeryId.HasValue)
            {
                surgeryPhrases = await SqlHelper.GetSurgeryPhrases(surgeryId.Value, user.ProviderID, user.LocationID);
                surgeryImages = await SqlHelper.GetSurgeryImages(surgeryId.Value, user.ProviderID, user.LocationID);
            }

            var result = new DebriefResult()
            {
                Flow = flow,
                PhraseCategories = categories,
                FlowPhrases = flowPhrases,
                SurgeryPhrases = surgeryPhrases,
                SmartPhrases = smartPhrases,
                FlowFeedback = flowFeedback,
                SurgeonNotes = surgeonNotes,
                FlowSteps = flowSteps,
                Messages = messages,
                FlowImages = flowImages,
                SurgeryImages = surgeryImages
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("GetSurgeryRoomSummary")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomSummary>))]
        [Route("api/surgery/roomSummary", Name = "GetSurgeryRoomSummary")]
        public async Task<HttpResponseMessage> GetSurgeryRoomSummary(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

            var result = await SqlHelper.GetSurgeryRoomSummary(surgeryId, user.ProviderID, user.LocationID);

            foreach (var surgery in result)
            {
                surgery.Patient = await SecureSqlHelper.GetPatient(surgery.PatientID,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID,
                    user.SecureDatabaseName);

                if (surgery.TotalMinutes == null)
                    continue;

                var nextStarts = result.Where(s => s.ScheduleDateTime > surgery.ScheduleDateTime).ToList();

                if (!nextStarts.Any())
                    continue;

                var nextStart = nextStarts.Min(s => s.ScheduleDateTime);

                var finishTime = surgery.ScheduleDateTime.AddMinutes(surgery.TotalMinutes.Value);
                var idleMinutes = nextStart.Subtract(finishTime).Minutes;

                if (idleMinutes > 0)
                    surgery.IdleMinutes = idleMinutes;
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("GetSurgeryRoomOverview")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomOverview>))]
        [Route("api/surgery/roomOverview", Name = "GetSurgeryRoomOverview")]
        public async Task<HttpResponseMessage> GetSurgeryRoomOverview(DateTime surgeryDate, int? specialtyId = null, int? roomGroupId = null, int? roomId = null, int? surgeonId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

            int? bundleId = null;
            int? procedureId = null;

            var surgeries = await SqlHelper.GetSurgeryRoomOverview(
                specialtyId, roomGroupId, roomId, surgeonId, 
                surgeryDate, user.ProviderID, user.LocationID);

            foreach (var surgery in surgeries)
            {
                surgery.Patient = await SecureSqlHelper.GetPatient(surgery.PatientID,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID,
                    user.SecureDatabaseName);

                if (surgery.TotalMinutes == null)
                    continue;

                var nextStarts = surgeries.Where(s => s.ScheduleDateTime > surgery.ScheduleDateTime).ToList();

                if (!nextStarts.Any())
                    continue;

                var nextStart = nextStarts.Min(s => s.ScheduleDateTime);

                var finishTime = surgery.ScheduleDateTime.AddMinutes(surgery.TotalMinutes.Value);
                var idleMinutes = nextStart.Subtract(finishTime).Minutes;

                if (idleMinutes > 0)
                    surgery.IdleMinutes = idleMinutes;
            }

            var result = surgeries.GroupBy(s => new {s.RoomID, s.RoomDescription})
                .Select(r => new RoomOverview()
                {
                    RoomID = r.Key.RoomID ?? 0,
                    RoomDescription = r.Key.RoomDescription,
                    RoomHours = RoomHour.Summarize(r.ToList())
                })
                .ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("UpdateDebrief")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/surgery/debrief", Name = "UpdateDebrief")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateDebrief(int surgeryPhraseId, int stepId, int roleId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.SurgeryPhraseDebrief(surgeryPhraseId, stepId, roleId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateCaseNotes")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/surgery/caseNotes", Name = "UpdateCaseNotes")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateCaseNotes(int surgeryId, [FromBody]DebriefUpdatePost update)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.SurgeryUpdateCaseNotes(surgeryId, update.CaseNotes, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AddSurgerySmartPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [Route("api/surgery/surgeryPhrase", Name = "AddSurgerySmartPhrase")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddSurgerySmartPhrase(int surgeryId, int smartPhraseId)
        {
            var user = await CacheUtil.GetUserSecurity();

            try
            {
                await SqlHelper.AddSurgerySmartPhrase(surgeryId, smartPhraseId,
                    user.ProviderID, user.LocationID);
            }
            catch (SqlException sqlEx)
            {
                if (sqlEx.Message.ToUpper().Contains("UNIQUE"))
                {
                    return Request.CreateResponse(HttpStatusCode.Conflict,
                        "Cannot assign instruction to step multiple times");
                }

                throw;
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/surgeryPhrase", Name = "UpdateSurgeryPhrase")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateSurgeryPhrase(int surgeryId, int surgeryPhraseId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateSurgeryPhrase(surgeryId, surgeryPhraseId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeryPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/surgeryPhrase", Name = "DeleteSurgeryPhrase")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeryPhrase(int surgeryId, int smartPhraseId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSurgeryPhrase(surgeryId, smartPhraseId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/surgeonNote", Name = "NewSurgeonNote")]
        [HttpPost]
        public async Task<IHttpActionResult> NewSurgeonNote([FromBody]SurgeonNotePost smartPhrase)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.NewSurgeonNote(smartPhrase.Phrase, smartPhrase.FlowID, smartPhrase.StepID, smartPhrase.RoleID,
                user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/surgeonNote", Name = "UpdateSurgeonNote")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateSurgeonNote(int surgeonNoteId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateSurgeonNote(surgeonNoteId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/surgeonNote", Name = "DeleteSurgeonNote")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeonNote(int surgeonNoteId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSurgeonNote(surgeonNoteId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignCard")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignCard", Name = "AssignCard")]
        public async Task<IHttpActionResult> AssignToCard(int surgeryId, int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AssignCardToCase(cardId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignRoomSetup", Name = "AssignRoomSetupCase")]
        public async Task<IHttpActionResult> AssignRoomSetupToCase(int surgeryId, int roomSetupId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AssignRoomSetupToCase(roomSetupId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/assignFlow", Name = "AssignFlowCase")]
        public async Task<IHttpActionResult> AssignToFlowCase(int surgeryId, int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AssignFlowToCase(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]SurgeryPost surgery)
        {
            var secureUser = await CacheUtil.GetUserSecurity();
            var user = await SqlHelper.GetUser(secureUser.ProviderID, secureUser.LocationID,  secureUser.UserID);

            var patientId = await SecureSqlHelper.CreatePatient(surgery.PtAcctNbr,
                surgery.PtDOB, surgery.PtGender, surgery.PtFirstName, surgery.PtLastName, surgery.PtMiddleInitial, surgery.PtBMI, 
                user.UserID, user.FirstName, user.LastName, (int)user.RoleID,
                secureUser.SecureDatabaseName);

            var caseId = await SqlHelper.CreateCase(patientId, secureUser.UserID, surgery.SpecialtyID, secureUser.ProviderID,
                secureUser.LocationID, surgery.CaseNbr);

            var cardFlowRoom = (surgery.BundleID.HasValue && surgery.SurgeonUserID.HasValue) ? 
                await SqlHelper.GetBundleDefaultCardFlowRoom(surgery.BundleID.Value, surgery.SurgeonUserID.Value, secureUser.ProviderID, secureUser.LocationID) : 
                (await SqlHelper.GetProcedureDefaultCardFlowRoom(secureUser.ProviderID, secureUser.LocationID, surgery.CptCode)).FirstOrDefault();

            var surgeryId = await SqlHelper.CreateSurgery(surgery, secureUser.ProviderID, secureUser.LocationID, patientId, caseId, 
                surgery.CardID ?? cardFlowRoom?.CardID, cardFlowRoom?.TemplateFlowID, cardFlowRoom?.TemplateRoomSetupID);

            return Request.CreateResponse(HttpStatusCode.Created, surgeryId);
        }

        // POST api/values
        [SwaggerOperation("AddCustomSurgeryItem")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/addSurgeryItemUse", Name = "AddCustomSurgeryItem")]
        public async Task<HttpResponseMessage> AddCustomSurgeryItem(int surgeryId, [FromBody]SurgeryCustomItemPost customItem)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (customItem.TrayID.HasValue)
            {
                await SqlHelper.AddCustomSurgeryTrayItem(surgeryId, customItem.TrayID ?? 0, customItem.ItemID, customItem.Quantity, user.ProviderID, user.LocationID);
            }
            else
            {
                await SqlHelper.AddCustomSurgeryItem(surgeryId, customItem.ItemID, customItem.Quantity, user.ProviderID, user.LocationID);
            }
        
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        [SwaggerOperation("UpdateTeams")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/Surgery/updateTeams")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateTeams([FromBody]SurgeryTeamUpdate teamUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();

            foreach (var surgeryId in teamUpdate.Surgeries)
            {
                foreach (var edit in teamUpdate.Edits)
                {
                    if (edit.Assign)
                    {
                        await SqlHelper.AddSurgeryUser(surgeryId, edit.UserID, user.ProviderID, user.LocationID);
                    }
                    else
                    {
                        await SqlHelper.DeleteSurgeryUser(surgeryId, edit.UserID, user.ProviderID, user.LocationID);
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddSurgeryUser")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/user", Name = "AddSurgeryUser")]
        public async Task<HttpResponseMessage> AddSurgeryUser(int surgeryId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AddSurgeryUser(surgeryId, userId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("NotifyAdministrator")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/notifyAdministrator", Name = "NotifyAdministrator")]
        public async Task<HttpResponseMessage> NotifyAdministrator(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var surgery = await SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);
            if (surgery == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, 0);

            var helpString = "Help needed with surgery in room: " + surgery.RoomDescription;
            var administrator = "16127016239";

            SmsNotification.NotifyUser(administrator, helpString);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeryUser")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        [Route("api/surgery/user", Name = "DeleteSurgeryUser")]
        public async Task<HttpResponseMessage> DeleteSurgeryUser(int surgeryId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSurgeryUser(surgeryId, userId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddSurgeryProcedure")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/surgeryProcedure", Name = "AddSurgeryProcedure")]
        public async Task<HttpResponseMessage> AddSurgeryProcedure(int surgeryId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.AddSurgeryProcedure(surgeryId, procedureId, user.ProviderID, user.LocationID);
        
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryProcedure")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPut]
        [Route("api/surgery/surgeryProcedure", Name = "UpdateSurgeryProcedure")]
        public async Task<HttpResponseMessage> UpdateSurgeryProcedure(int surgeryId, string cptCode, [FromBody]SurgeryProcedureEditPost procedureEdit)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateSurgeryProcedure(surgeryId, cptCode, procedureEdit.IsPerformed ? "Y" : "N", user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeryProcedure")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        [Route("api/surgery/surgeryProcedure", Name = "DeleteSurgeryProcedure")]
        public async Task<HttpResponseMessage> DeleteSurgeryProcedure(int surgeryId, string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSurgeryProcedure(surgeryId, cptCode, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("NewSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("api/surgery/surgeryImage", Name = "NewSurgeryImage")]
        [HttpPut]
        public async Task<IHttpActionResult> NewSurgeryImage(int surgeryId, int stepId, int roleId, string comment)
        {
            var user = await CacheUtil.GetUserSecurity();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // extract file name and file contents
            var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                .FirstOrDefault(p => p.Name.ToLower() == "filename");
            var fileName = fileNameParam?.Value.Trim('"') ?? "";
            var fileExtension = Path.GetExtension(fileName);
            var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

            if (fileExtension == ".png" ||
                fileExtension == ".jpg" ||
                fileExtension == ".jpeg")
            {
                fileContents = BlobStorageHelper.CompressImage(fileContents);
            }

            var surgeryImageId = await SqlHelper.NewSurgeryImage(surgeryId, stepId, roleId, comment,
                user.ProviderID, user.LocationID);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.PutBlobBytes(folder, surgeryImageId.ToString(), fileContents);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/surgery/surgeryImage", Name = "UpdateSurgeryImage")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateSurgeryImage(int surgeryImageId, [FromBody]FlowImagePost surgeryImage)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateSurgeryImage(surgeryImageId, surgeryImage.Comment, surgeryImage.StepID, surgeryImage.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("RotateSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/flow/rotateSurgeryImage", Name = "RotateSurgeryImage")]
        [HttpPut]
        public async Task<IHttpActionResult> RotateSurgeryImage(int surgeryImageId, int surgeryId, int direction)
        {
            var user = await CacheUtil.GetUserSecurity();

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.RotateImage(folder, surgeryImageId.ToString(), direction);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("api/surgery/surgeryImage", Name = "DeleteSurgeryImage")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeryImage(int surgeryImageId, int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.DeleteSurgeryImage(surgeryImageId, user.ProviderID, user.LocationID);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.DeleteBlob(folder, surgeryImageId.ToString());

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryCountsUsages")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/updateCountsUsages", Name = "UpdateSurgeryCountsUsages")]
        public async Task<HttpResponseMessage> UpdateSurgeryCountsUsages(int surgeryId, [FromBody]SurgeryCountPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            try
            {
                await SqlHelper.UpdateSurgeryCount(surgeryId, post.ItemCounts, user.ProviderID, user.LocationID);
                await SqlHelper.UpdateSurgeryInstrumentCount(surgeryId, post.InstrumentCounts, user.ProviderID, user.LocationID);

                if (post?.Answers.Any() == true)
                {
                    await SqlHelper.UpdateSurgeryQuestionAnswers(surgeryId, post.Answers, user.ProviderID, user.LocationID);
                }
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
                throw;
            }

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryCounts")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/updateCounts", Name = "UpdateSurgeryCounts")]
        public async Task<HttpResponseMessage> UpdateSurgeryCounts(int surgeryId, int sharpCount, int needleCount, int lapCount, int specimenCount)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateSurgeryHeaderCounts(surgeryId, sharpCount, needleCount, lapCount, specimenCount, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("UpdateStaffChange")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/updateStaffChange", Name = "UpdateStaffChange")]
        public async Task<HttpResponseMessage> UpdateStaffChange(int surgeryId, string staffChange)
        {
            var user = await CacheUtil.GetUserSecurity();

            await SqlHelper.UpdateStaffChange(surgeryId, staffChange, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("SurgeryReviewComplete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/surgery/reviewComplete", Name = "SurgeryReviewComplete")]
        public async Task<HttpResponseMessage> SurgeryReviewComplete(int surgeryId, DateTime reviewComplete)
        {
            var user = await CacheUtil.GetUserSecurity();

            var success = await SqlHelper.SurgeryReviewComplete(surgeryId, reviewComplete, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, success);
        }

        // POST api/values
        [SwaggerOperation("EditSurgeryProperties")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        [Route("api/surgery/editProperties", Name = "EditSurgeryProperties")]
        public async Task<HttpResponseMessage> EditSurgeryProperties(int surgeryId, [FromBody]SurgeryEditPost surgeryEditPost)
        {
            var user = await CacheUtil.GetUserSecurity();
            var success = await SqlHelper.SurgeryEditProperties(surgeryId,
                surgeryEditPost.RoomID, surgeryEditPost.ScheduleDateTime, user.ProviderID, user.LocationID);

            if (!surgeryEditPost.NotificationUser.HasValue) return Request.CreateResponse(HttpStatusCode.OK, success);


            var surgery = await SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);
            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            var patient = await SecureSqlHelper.GetPatient(surgery.PatientID,
                user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID, 
                user.SecureDatabaseName);

            var message = $"Surgery #{surgery.CaseNumber} patient {patient.LastName} room {surgery.RoomDescription} {surgery.ScheduleTime:hh\\:mm} modified - please review schedule";

            var notificationUser = await SqlHelper.GetUser(user.ProviderID, user.LocationID,  surgeryEditPost.NotificationUser.Value);
            if (notificationUser?.CellPhone != null)
                SmsNotification.NotifyUser(notificationUser.CellPhone, message);

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
        public async Task<HttpResponseMessage> Delete(int id)
        {
            var user = await CacheUtil.GetUserSecurity();

            var success = await SqlHelper.DeleteSurgery(id, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, success);
        }
    }
}