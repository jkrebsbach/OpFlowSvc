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
    [RoutePrefix("api/surgery")]
    public class SurgeryController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetSurgery")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PatientSurgery))]
        public async Task<HttpResponseMessage> GetSurgery(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            //var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            //var userObject = await sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            var patientSurgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            //patientSurgery.Patient =
            //    await secureSqlHelper.GetPatient(patientSurgery.PatientID,
            //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

            return Request.CreateResponse(HttpStatusCode.OK, patientSurgery);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetNewSurgerySetup")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NewSurgerySetup))]
        [Route("newSurgerySetup")]
        public async Task<HttpResponseMessage> GetNewSurgerySetup()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var rooms = await sqlHelper.GetRooms(user.SelectedLocation);
            var specialties = await sqlHelper.GetSpecialties(user.SelectedLocation);
            var lateralities = await sqlHelper.GetLateralities(user.SelectedLocation);
            var surgeons = await sqlHelper.GetSurgeryUsers(user.SelectedLocation);
            var proposals = await sqlHelper.GetProposedTrays(null, user.SelectedLocation);

            var profiles = await sqlHelper.GetProcedureProfiles();
            
            var result = new NewSurgerySetup()
            {
                Rooms = rooms,
                Specialties = specialties,
                Lateralities = lateralities,
                Surgeons = surgeons,
                Profiles = profiles,
                Proposals = proposals
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetNewSurgeryVendor")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(NewSurgeryVendor))]
        [Route("newSurgeryVendor")]
        public async Task<HttpResponseMessage> GetNewSurgeryVendor()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (!user.Vendor  && user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            
            var providers = await sqlHelper.GetOpFlowSetup();
            var caseProfiles = await sqlHelper.GetCaseProfiles(user.SelectedLocation);
            var trayGroups = await sqlHelper.GetTrayGroups(user.SelectedLocation);

            var result = new NewSurgeryVendor()
            {
                Providers = providers,
                CaseProfiles = caseProfiles,
                TrayGroups = trayGroups
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetCase")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Surgery))]
        [Route("case")]
        public async Task<HttpResponseMessage> GetCase(int caseId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var schedules = await sqlHelper.GetCase(caseId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetSurgeryMetrics")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(PatientSurgery))]
        [Route("metrics")]
        public async Task<HttpResponseMessage> GetSurgeryMetrics(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            
            var surgeryMetrics = await sqlHelper.GetSurgeryMetrics(surgeryId, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK,
            new {
                Metrics = surgeryMetrics
            });
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchCategory>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        [Route("searchCases")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetCases([FromBody] SearchCasePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var schedules = await sqlHelper.SearchCases(post.UserID, post.surgeonUserId, post.roomGroupId, post.roomId,
                null, post.procedureId, post.specialtyId,
                post.BegDate, post.EndDate, user.SelectedLocation);

            await sqlHelper.LoadProposalCounts(schedules, user.SelectedLocation);

            if (post.CountStatus == "Complete")
                schedules = schedules.Where(s => s.SurgeryCountType != null).ToList();
            
            if (post.CountStatus == "Count")
                schedules = schedules.Where(s => s.CountSurgery).ToList();
            
            if (post.CountStatus == "Audit")
                schedules = schedules.Where(s => s.AuditSurgery).ToList();
            
            var groups = new[] { "Counts", "Audits", "Untargeted", "Completed" };

            var result = new List<SurgerySearchCategory>();
            foreach (var group in groups)
            {
                result.Add(new SurgerySearchCategory()
                {
                    CategoryName = group,
                    Schedule = schedules.Where(s => s.CaseCategory == group).ToList()
                });
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("SearchSurgeonCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySearchResult>))]
        [Route("searchSurgeonCases")]
        public async Task<HttpResponseMessage> GetSurgeonCases(int surgeonUserId, DateTime begDate, DateTime endDate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var schedules = await sqlHelper.GetSurgeonCases(surgeonUserId, begDate, endDate, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetSurgeonPreferences")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeonPreference>))]
        [Route("surgeonPreferences")]
        public async Task<HttpResponseMessage> GetSurgeonPreferences(int? surgeryId = null, int? userId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var preferences = await sqlHelper.GetSurgeonPreferences(user.SelectedLocation);

            int? caseProfileId = null;
            var users = new List<int>();
            var surgeonPreferences = new List<SurgeonPreference>();

            if (surgeryId.HasValue)
            {
                var surgery = await sqlHelper.GetSurgery(surgeryId.Value, user.SelectedLocation);
                var schedules = await sqlHelper.GetSurgeryUsers(surgeryId.Value, user.SelectedLocation);

                caseProfileId = surgery.CaseProfileID;

                users.Add(surgery.UserID);
                users.AddRange(schedules.Select(s => s.UserID));

                surgeonPreferences = preferences.Where(sp =>
                    (users.Any(s => s == sp.SurgeonID)) &&
                    (sp.CaseProfileID == null || sp.CaseProfileID == caseProfileId)).ToList();
            }
            if (userId.HasValue)
            {
                surgeonPreferences = preferences.Where(sp =>
                    sp.SurgeonID == userId.Value).ToList();
            }

            // Preference where primary surgeon or any surgeon on case

            return Request.CreateResponse(HttpStatusCode.OK, new {
                SurgeonPreferences = surgeonPreferences
            });
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("UpdateSurgeonPreferences")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeonPreference>))]
        [Route("surgeonPreference")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateSurgeonPreferences(int surgeryId, int surgeonPreferenceId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSurgeonPreferences(user.SelectedLocation);
            var preference = result.FirstOrDefault(sp => sp.SurgeonPreferenceID == surgeonPreferenceId);
            if (preference == null)
                return Request.CreateResponse(HttpStatusCode.OK, result);

            var trayGroups = await sqlHelper.GetTrayGroups(user.SelectedLocation);

            foreach (var trayGroupId in preference.TrayGroupID)
            {
                var trayGroup = trayGroups.FirstOrDefault(tg => tg.TrayGroupID == trayGroupId);
                foreach (var tray in trayGroup.Trays)
                {
                    await sqlHelper.AddCustomSurgeryItem(surgeryId, tray.TrayItemID, 1, user.SelectedLocation);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        
        // GET api/surgery?userId=5
        [SwaggerOperation("GetSchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgerySchedule>))]
        [Route("cases")]
        public async Task<HttpResponseMessage> GetSurgerySchedule(DateTime? scheduleDate = null, int? roomId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            int? userId = null;
            if (roomId == null)
                userId = user.UserID;

            var surgeries = await sqlHelper.GetScheduledSurgeries(userId, scheduleDate, roomId, user.SelectedLocation);

            foreach (var surgery in surgeries)
            {
                surgery.SurgeryUsers =
                    await sqlHelper.GetSurgeryUsers(surgery.CaseID, user.SelectedLocation);
            }

            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetAlerts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgery>))]
        [Route("alerts")]
        public async Task<HttpResponseMessage> GetSurgeryAlerts(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var schedules = await sqlHelper.GetSurgeryAlerts(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<BundleProcedure>))]
        [Route("procedures")]
        public async Task<HttpResponseMessage> GetSurgeryProcedures(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var procedures = await sqlHelper.GetSurgeryProcedures(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, procedures);
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetDelayReasons")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryDelayReason>))]
        [Route("delayReasons")]
        public async Task<HttpResponseMessage> GetSurgeryDelayReasons()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var reasons = await sqlHelper.GetSurgeryDelayReasons(user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, reasons);
        }

        [SwaggerOperation("GetSurgeryUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryUser>))]
        [Route("users")]
        public async Task<HttpResponseMessage> GetSurgeryUsers(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var schedules = await sqlHelper.GetSurgeryUsers(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        [SwaggerOperation("GetVendorReps")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryVendorRep>))]
        [Route("VendorReps")]
        public async Task<HttpResponseMessage> GetSurgeryVendorReps(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var schedules = await sqlHelper.GetSurgeryVendorReps(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, schedules);
        }

        // GET api/values/5
        [SwaggerOperation("GetSurgeryCardList")]
        [Route("cards")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCard>))]
        public async Task<HttpResponseMessage> GetSurgeryCardList(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSurgeryCardList(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetSurgeryFlowList")]
        [Route("flows")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCard>))]
        public async Task<HttpResponseMessage> GetSurgeryFlowList(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSurgeryFlowList(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItems")]
        [Route("carditems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryCardItem>))]
        public async Task<HttpResponseMessage> GetCardItems(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSurgeryCardItems(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItemCounts")]
        [Route("cardItemCounts")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardItemCountResult))]
        public async Task<HttpResponseMessage> GetSurgeryCardItemCounts(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            var cardItemCounts = await sqlHelper.GetSurgeryCardItemCounts(surgeryId, user.SelectedLocation);
            var itemCounts = cardItemCounts.CardItemCounts.GroupBy(ic => new { ic.ItemType, ic.TrayName, ic.TrayID });

            var trayOpens = await sqlHelper.GetSurgeryTrayOpens(surgeryId, user.SelectedLocation);

            var result = new CardItemCountResult()
            {
                Surgery = surgery,
                Collections = new List<TrayCollection>(),
                Trays = new List<TrayUsage>(),
                FlowSteps = await sqlHelper.GetSurgeryFlowList(surgeryId, user.SelectedLocation),
                Sutures = await sqlHelper.GetItemSutures(user.SelectedLocation),
                CptCodes = await sqlHelper.GetSurgeryCPTCodes(surgeryId, user.SelectedLocation),
                SutureCounts = await sqlHelper.GetSurgerySutureCounts(surgeryId, user.SelectedLocation)
            };

            foreach (var countType in itemCounts)
            {
                switch (countType.Key.ItemType)
                {
                    case "ROOM":
                        result.Room = countType.ToList();
                        break;
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
                    case "PROPOSAL":
                        result.ProposedTrays.Add(new TrayUsage()
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
                if (tray == null)
                {
                    // If tray null, likely it was a vendor tray
                    var coll = result.Collections.FirstOrDefault(c => c.ItemID == trayOpen.TrayID);
                }
                else
                {
                    tray.TrayOpened = trayOpen.TrayOpened;
                    tray.Feedback = trayOpen.Feedback;
                }
            }



            return Request.CreateResponse(HttpStatusCode.OK, result);
        }


        [SwaggerOperation("UpdateTrayOpen")]
        [Route("updateTrayOpen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        public async Task<HttpResponseMessage> UpdateSurgeryTrayOpen(int surgeryId, int trayId, bool trayOpened)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateSurgeryTrayOpens(surgeryId, user.ProviderID, user.LocationID, trayId, trayOpened);

            return Request.CreateResponse(HttpStatusCode.OK, result);

        }

        // GET api/values/5
        [SwaggerOperation("GetSearchScreen")]
        [Route("searchScreen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SearchScreen))]
        public async Task<HttpResponseMessage> GetSearchScreen()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var users = await sqlHelper.GetSurgeryUsers(user.SelectedLocation);

            var setup = await sqlHelper.GetOpFlowSetup();
            var location = setup.FirstOrDefault(p => p.ProviderID == user.ProviderID)?
                .Locations.FirstOrDefault(l => l.LocationID == user.LocationID);

            var result = new SearchScreen
            {
                Rooms = await sqlHelper.GetRooms(user.SelectedLocation),
                RoomGroups = await sqlHelper.GetRoomGroups(user.SelectedLocation),
                Specialties = await sqlHelper.GetSpecialties(user.SelectedLocation),
                Users = users,
                Surgeons = users.Where(u => u.RoleID == RoleEnum.Surgeon).ToList(),
                Trays = await sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation),
                Proposals = await sqlHelper.GetProposedTrays(null, user.SelectedLocation),
                Bundles = await sqlHelper.GetBundles(null, user.ProviderID, user.LocationID),
                Procedures = await sqlHelper.GetProcedures(null, user.ProviderID, user.LocationID),
                LocationSetup = location
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetDebriefScreen")]
        [Route("debriefScreen")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DebriefResult))]
        public async Task<HttpResponseMessage> GetDebriefScreen(int flowId, int? surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var flow = await sqlHelper.GetFlow(flowId, user.SelectedLocation);
            var categories = await sqlHelper.GetSmartPhraseCategories(user.SelectedLocation);
            var flowPhrases = await sqlHelper.GetFlowPhrases(flowId, user.ProviderID, user.LocationID);

            var smartPhrases = new List<SmartPhrase>();
            if (flow != null)
                smartPhrases = await sqlHelper.GetSmartPhrases(null, null, flow.OwnerUserID, user.SelectedLocation);

            var flowFeedback = await sqlHelper.GetFlowFeedback(flowId, user.SelectedLocation);
            var surgeonNotes = await sqlHelper.GetSurgeonNotes(flowId, user.ProviderID, user.LocationID);
            var flowSteps = await sqlHelper.GetFlowTimings(flowId, user.SelectedLocation);
            var messages =
                await sqlHelper.GetMessaging(user.UserID, surgeryId, null, null, user.ProviderID, user.LocationID);

            var flowImages = await sqlHelper.GetFlowImages(flowId, user.SelectedLocation);

            var surgeryPhrases = new List<SurgeryPhrase>();
            var surgeryImages = new List<SurgeryImage>();

            if (surgeryId.HasValue)
            {
                surgeryPhrases = await sqlHelper.GetSurgeryPhrases(surgeryId.Value, user.ProviderID, user.LocationID);
                surgeryImages = await sqlHelper.GetSurgeryImages(surgeryId.Value, user.SelectedLocation);
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

        // GET api/values/5
        [SwaggerOperation("GetTrayAudits")]
        [Route("trayAudits")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DebriefResult))]
        public async Task<HttpResponseMessage> GetTrayAudits(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var instrumentLookups = await sqlHelper.GetTrayInstrumentLookups(user.SelectedLocation);
            var audits = await sqlHelper.GetSurgeryProposedTrays(surgeryId, user.SelectedLocation);
            var groups = await sqlHelper.GetTrayGroups(user.SelectedLocation);
            var caseProfile = await sqlHelper.GetSurgeryCaseProfile(surgeryId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                TrayGroups = groups,
                Categories = instrumentLookups.Categories,
                Eponyms = instrumentLookups.Eponyms,
                Types = instrumentLookups.Types,
                Audits = audits.Audits,
                ScrubTechs = audits.ScrubTechs,
                CaseProfile = caseProfile,
            });
        }

        // POST api/values
        [SwaggerOperation("GetSurgeryRoomSummary")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RoomSummary>))]
        [Route("roomSummary", Name = "GetSurgeryRoomSummary")]
        public async Task<HttpResponseMessage> GetSurgeryRoomSummary(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var userObject = await sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            var result = await sqlHelper.GetSurgeryRoomSummary(surgeryId, user.ProviderID, user.LocationID);

            foreach (var surgery in result)
            {
                surgery.Patient = await secureSqlHelper.GetPatient(surgery.PatientID,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

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
        [Route("roomOverview", Name = "GetSurgeryRoomOverview")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetSurgeryRoomOverview([FromBody] RoomOverviewPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var userObject = await sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            var surgeries = await sqlHelper.GetSurgeryRoomOverview(
                post.SpecialtyId, post.RoomGroupId, post.RoomId, post.SurgeonId,
                post.SurgeryDate, user.ProviderID, user.LocationID);

            if (post.CountStatus?.Any() == true)
            {
                var counts = post.CountStatus.Any(c => c == "Count");
                var audits = post.CountStatus.Any(c => c == "Audit");
                var complete = post.CountStatus.Any(c => c == "Complete");
                surgeries = surgeries.Where(s => (audits && s.AuditSurgery) || (counts && s.CountSurgery)).ToList();

                if (complete)
                    surgeries = surgeries.Where(s => s.SurgeryCountType != null).ToList();
            }

            foreach (var surgery in surgeries)
            {
                surgery.Patient = await secureSqlHelper.GetPatient(surgery.PatientID,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

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
        [SwaggerOperation("UpdateEstCompTime")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("estCompTime/{surgeryId}", Name = "UpdateEstCompTime")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateEstCompTime(int surgeryId, DateTime estCompTime)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryEstCompTime(surgeryId, estCompTime, user.ProviderID, user.LocationID);

            return await GetSurgeryCardItemCounts(surgeryId);
        }

        // POST api/values
        [SwaggerOperation("UpdateTrayFeedback")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("trayFeedback", Name = "UpdateTrayFeedback")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateTrayFeedback(int surgeryId, int trayId, [FromBody] SurgeryTrayFeedback post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryTrayFeedback(surgeryId, trayId, post.Feedback, user.ProviderID, user.LocationID);

            return await GetSurgeryCardItemCounts(surgeryId);
        }

        // POST api/values
        [SwaggerOperation("UpdateDebrief")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("debrief", Name = "UpdateDebrief")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateDebrief(int surgeryPhraseId, int stepId, int roleId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.SurgeryPhraseDebrief(surgeryPhraseId, stepId, roleId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateCaseNotes")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("caseNotes", Name = "UpdateCaseNotes")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateCaseNotes(int surgeryId, [FromBody]DebriefUpdatePost update)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.SurgeryUpdateCaseNotes(surgeryId, update.CaseNotes, user.ProviderID, user.LocationID);

            return Ok();
        }

        [SwaggerOperation("UpdatePerioperativeCaseProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("perioperativeCaseProfile")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdatePerioperativeCaseProfile(int surgeryId, [FromBody] CaseProfileSchedulePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryPerioperativeCaseProfile(surgeryId, post.Questions, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, surgeryId);
        }

        // POST api/values
        [SwaggerOperation("AddSurgerySmartPhrase")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Conflict)]
        [Route("surgeryPhrase", Name = "AddSurgerySmartPhrase")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddSurgerySmartPhrase(int surgeryId, int smartPhraseId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            try
            {
                await sqlHelper.AddSurgerySmartPhrase(surgeryId, smartPhraseId,
                    user.SelectedLocation);
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
        [Route("surgeryPhrase", Name = "UpdateSurgeryPhrase")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateSurgeryPhrase(int surgeryId, int surgeryPhraseId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryPhrase(surgeryId, surgeryPhraseId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeryPhrase")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("surgeryPhrase", Name = "DeleteSurgeryPhrase")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeryPhrase(int surgeryId, int smartPhraseId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteSurgeryPhrase(surgeryId, smartPhraseId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("NewSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("surgeonNote", Name = "NewSurgeonNote")]
        [HttpPost]
        public async Task<IHttpActionResult> NewSurgeonNote([FromBody]SurgeonNotePost smartPhrase)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.NewSurgeonNote(smartPhrase.Phrase, smartPhrase.FlowID, smartPhrase.StepID, smartPhrase.RoleID,
                user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("surgeonNote", Name = "UpdateSurgeonNote")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateSurgeonNote(int surgeonNoteId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeonNote(surgeonNoteId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeonNote")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("surgeonNote", Name = "DeleteSurgeonNote")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeonNote(int surgeonNoteId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteSurgeonNote(surgeonNoteId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignCard")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignCard", Name = "AssignCard")]
        public async Task<IHttpActionResult> AssignToCard(int surgeryId, int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignCardToCase(cardId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignRoomSetup", Name = "AssignRoomSetupCase")]
        public async Task<IHttpActionResult> AssignRoomSetupToCase(int surgeryId, int roomSetupId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignRoomSetupToCase(roomSetupId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignFlow", Name = "AssignFlowCase")]
        public async Task<IHttpActionResult> AssignToFlowCase(int surgeryId, int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignFlowToCase(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]SurgeryPost surgery)
        {
            try
            {
                var secureUser = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();
                var secureSqlHelper = new SecureSqlHelper(secureUser.SecureDatabaseName);

                if (!secureUser.Vendor)
                    surgery.VendorLocationID = null;

                var user = await sqlHelper.GetUser(secureUser.SelectedLocation, secureUser.UserID);

                //var patientId = await secureSqlHelper.CreatePatient(surgery.PtAcctNbr,
                //    surgery.PtDOB, surgery.PtGender, surgery.PtFirstName, surgery.PtLastName, surgery.PtMiddleInitial, surgery.PtBMI,
                //    user.UserID, user.FirstName, user.LastName, (int)user.RoleID);
                int? patientId = null;

                var caseId = await sqlHelper.CreateCase(patientId, secureUser.UserID, surgery.SpecialtyID,
                    secureUser.SelectedLocation, surgery.CaseNbr);

                var cardFlowRoom = (surgery.BundleID.HasValue && surgery.SurgeonUserID.HasValue) ?
                    await sqlHelper.GetBundleDefaultCardFlowRoom(surgery.BundleID.Value, surgery.SurgeonUserID.Value, secureUser.SelectedLocation) :
                    (await sqlHelper.GetProcedureDefaultCardFlowRoom(surgery.CptCode, secureUser.SelectedLocation)).FirstOrDefault();

                var surgeryId = await sqlHelper.CreateSurgery(surgery, patientId, caseId,
                    surgery.CardID ?? cardFlowRoom?.CardID, cardFlowRoom?.TemplateFlowID, cardFlowRoom?.TemplateRoomSetupID,
                    secureUser.SelectedLocation);

                if (surgery.SecondarySurgeons != null && surgery.SecondarySurgeons.Any())
                {
                    foreach (var userId in surgery.SecondarySurgeons)
                    {
                        await sqlHelper.AddSurgeryUser(surgeryId, userId, secureUser.SelectedLocation);
                    }
                }

                if (surgery.TrayGroupID != null && surgery.TrayGroupID.Any())
                {
                    var trayGroups = await sqlHelper.GetTrayGroups(secureUser.SelectedLocation);

                    foreach (var trayGroup in trayGroups.Where(t => surgery.TrayGroupID.Any(stg => t.TrayGroupID == stg)))
                    {
                        foreach (var tray in trayGroup.Trays)
                        {
                            await sqlHelper.AddCustomSurgeryItem(surgeryId, tray.TrayItemID, 1, secureUser.SelectedLocation);
                        }
                    }
                }

                if (surgery.TrayProposalID != null)
                {
                    await sqlHelper.UpdateProposedTrayCount(surgery.TrayProposalID.Value, surgeryId, null, null, secureUser.SelectedLocation);
                }

                return Request.CreateResponse(HttpStatusCode.Created, surgeryId);
            }
            catch(Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        // POST api/values
        [SwaggerOperation("AddCustomSurgeryItem")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("addSurgeryItemUse", Name = "AddCustomSurgeryItem")]
        public async Task<HttpResponseMessage> AddCustomSurgeryItem(int surgeryId, [FromBody]SurgeryCustomItemPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            foreach (var customItem in post.Items)
            {
                if (customItem.TrayID.HasValue)
                {
                    await sqlHelper.AddCustomSurgeryTrayItem(surgeryId, customItem.TrayID ?? 0, customItem.ItemID, customItem.Quantity, user.SelectedLocation);
                }
                else
                {
                    await sqlHelper.AddCustomSurgeryItem(surgeryId, customItem.ItemID, customItem.Quantity, user.SelectedLocation);
                }
            }
        
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddTemporaryTray")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("addTemporaryTray", Name = "AddTemporaryTray")]
        public async Task<HttpResponseMessage> AddTemporaryTray(int surgeryId, string trayName)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            trayName = $"SURG {surgeryId} TEMP: {trayName ?? string.Empty}";

            await sqlHelper.AddSurgeryTemporaryTray(surgeryId, trayName, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("addProposedTray", Name = "AddProposedTray")]
        public async Task<HttpResponseMessage> AddProposedTray(int surgeryId, int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AddSurgeryProposedTray(surgeryId, trayProposalId, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddProposedTrayGroup")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("addProposedTrayGroup", Name = "AddProposedTrayGroup")]
        public async Task<HttpResponseMessage> AddProposedTrayGroup(int surgeryId, int trayGroupId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AddSurgeryProposedTrayGroup(surgeryId, trayGroupId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        [SwaggerOperation("UpdateTeams")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("updateTeams")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateTeams([FromBody]SurgeryTeamUpdate teamUpdate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            foreach (var surgeryId in teamUpdate.Surgeries)
            {
                foreach (var edit in teamUpdate.Edits)
                {
                    if (edit.Assign)
                    {
                        await sqlHelper.AddSurgeryUser(surgeryId, edit.UserID, user.SelectedLocation);
                    }
                    else
                    {
                        await sqlHelper.DeleteSurgeryUser(surgeryId, edit.UserID, user.SelectedLocation);
                    }
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddSurgeryUser")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("user", Name = "AddSurgeryUser")]
        public async Task<HttpResponseMessage> AddSurgeryUser(int surgeryId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AddSurgeryUser(surgeryId, userId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("NotifyAdministrator")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("notifyAdministrator", Name = "NotifyAdministrator")]
        public async Task<HttpResponseMessage> NotifyAdministrator(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
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
        [Route("user", Name = "DeleteSurgeryUser")]
        public async Task<HttpResponseMessage> DeleteSurgeryUser(int surgeryId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteSurgeryUser(surgeryId, userId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("AddSurgeryProcedure")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("surgeryProcedure", Name = "AddSurgeryProcedure")]
        public async Task<HttpResponseMessage> AddSurgeryProcedure(int surgeryId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AddSurgeryProcedure(surgeryId, procedureId, user.SelectedLocation);
        
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryProcedure")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPut]
        [Route("surgeryProcedure", Name = "UpdateSurgeryProcedure")]
        public async Task<HttpResponseMessage> UpdateSurgeryProcedure(int surgeryId, string cptCode, [FromBody]SurgeryProcedureEditPost procedureEdit)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryProcedure(surgeryId, cptCode, procedureEdit.IsPerformed ? "Y" : "N", user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("DeleteSurgeryProcedure")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        [Route("surgeryProcedure", Name = "DeleteSurgeryProcedure")]
        public async Task<HttpResponseMessage> DeleteSurgeryProcedure(int surgeryId, string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteSurgeryProcedure(surgeryId, cptCode, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("NewSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("surgeryImage", Name = "NewSurgeryImage")]
        [HttpPut]
        public async Task<IHttpActionResult> NewSurgeryImage(int surgeryId, int stepId, int roleId, string comment)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null) return NotFound();

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

            var surgeryImageId = await sqlHelper.NewSurgeryImage(surgeryId, stepId, roleId, comment,
                user.SelectedLocation);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.PutBlobBytes(folder, surgeryImageId.ToString(), fileContents);

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("surgeryImage", Name = "UpdateSurgeryImage")]
        [HttpPost]
        public async Task<IHttpActionResult> UpdateSurgeryImage(int surgeryImageId, [FromBody]FlowImagePost surgeryImage)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryImage(surgeryImageId, surgeryImage.Comment, surgeryImage.StepID, surgeryImage.RoleID, user.SelectedLocation);

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
            var sqlHelper = new SqlHelper();

            var surgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null) return NotFound();

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
        [Route("surgeryImage", Name = "DeleteSurgeryImage")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteSurgeryImage(int surgeryImageId, int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null) return NotFound();

            await sqlHelper.DeleteSurgeryImage(surgeryImageId, user.SelectedLocation);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            await storageHelper.DeleteBlob(folder, surgeryImageId.ToString());

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("UpdateSurgeryCountsUsages")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("updateCountsUsages", Name = "UpdateSurgeryCountsUsages")]
        public async Task<HttpResponseMessage> UpdateSurgeryCountsUsages(int surgeryId, [FromBody]SurgeryCountSummaryPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            try
            {
                await sqlHelper.UpdateSurgeryCount(surgeryId, post.ItemCounts, post.CountComments, post.SurgeryType, user.SelectedLocation);
                await sqlHelper.UpdateSurgeryInstrumentCount(surgeryId, post.InstrumentCounts, user.SelectedLocation);
                await sqlHelper.UpdateSurgeryProposedCount(surgeryId, post.ProposedCounts, user.SelectedLocation);
                await sqlHelper.UpdateSurgerySutureCount(surgeryId, post.SutureCounts, post.DeletedSutures, user.SelectedLocation);
                await sqlHelper.UpdateSurgeryMetricAnswers(surgeryId, post.MetricAnswers, user.SelectedLocation);

                if (post?.Answers.Any() == true)
                {
                    await sqlHelper.UpdateSurgeryQuestionAnswers(surgeryId, post.Answers, user.SelectedLocation);
                }

                await sqlHelper.UpdateSurgeryCPTs(surgeryId, post.SurgeryCpts, user.SelectedLocation);
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
        [Route("updateCounts", Name = "UpdateSurgeryCounts")]
        public async Task<HttpResponseMessage> UpdateSurgeryCounts(int surgeryId, int sharpCount, int needleCount, int lapCount, int specimenCount)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryHeaderCounts(surgeryId, sharpCount, needleCount, lapCount, specimenCount, user.SelectedLocation);
            
            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("UpdateStaffChange")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("updateStaffChange", Name = "UpdateStaffChange")]
        public async Task<HttpResponseMessage> UpdateStaffChange(int surgeryId, string staffChange)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateStaffChange(surgeryId, staffChange, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, 0);
        }

        // POST api/values
        [SwaggerOperation("SurgeryReviewComplete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("reviewComplete", Name = "SurgeryReviewComplete")]
        public async Task<HttpResponseMessage> SurgeryReviewComplete(int surgeryId, DateTime reviewComplete)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var success = await sqlHelper.SurgeryReviewComplete(surgeryId, reviewComplete, user.UserID, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, success);
        }

        // POST api/values
        [SwaggerOperation("EditSurgeryProperties")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [HttpPost]
        [Route("editProperties", Name = "EditSurgeryProperties")]
        public async Task<HttpResponseMessage> EditSurgeryProperties(int surgeryId, [FromBody]SurgeryEditPost surgeryEditPost)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var success = await sqlHelper.SurgeryEditProperties(surgeryId,
                surgeryEditPost.RoomID, surgeryEditPost.ScheduleDateTime, user.SelectedLocation);

            if (!surgeryEditPost.NotificationUser.HasValue) return Request.CreateResponse(HttpStatusCode.OK, success);


            var surgery = await sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            var userObject = await sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

            var patient = await secureSqlHelper.GetPatient(surgery.PatientID,
                user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

            var message = $"Surgery #{surgery.CaseNumber} patient {patient.LastName} room {surgery.RoomDescription} {surgery.ScheduleTime:hh\\:mm} modified - please review schedule";

            var notificationUser = await sqlHelper.GetUser(user.SelectedLocation,  surgeryEditPost.NotificationUser.Value);
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
            var sqlHelper = new SqlHelper();

            var success = await sqlHelper.DeleteSurgery(id, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, success);
        }
    }
}