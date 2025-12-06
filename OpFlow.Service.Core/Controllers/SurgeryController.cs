using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Data.Debrief;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/surgery")]
    public class SurgeryController : OpFlowController
    {
        private BlobStorageHelper _blobStorageHelper;

        public SurgeryController(
             BlobStorageHelper blobStorageHelper,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
            _blobStorageHelper = blobStorageHelper;
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        public async Task<ActionResult> GetSurgery(int surgeryId)
        {
            var user = await GetUserSecurity();
            
            //var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            //var userObject = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            var patientSurgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            //patientSurgery.Patient =
            //    await secureSqlHelper.GetPatient(patientSurgery.PatientID,
            //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

            return Ok(patientSurgery);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("newSurgerySetup")]
        public async Task<ActionResult> GetNewSurgerySetup()
        {
            var user = await GetUserSecurity();
            

            var rooms = await _sqlHelper.GetRooms(user.SelectedLocation);
            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var lateralities = await _sqlHelper.GetLateralities(user.SelectedLocation);
            var surgeons = await _sqlHelper.GetSurgeryUsers(user.SelectedLocation);
            var proposals = await _sqlHelper.GetProposedTrays(null, user.SelectedLocation);

            var profiles = await _sqlHelper.GetProcedureProfiles();
            
            var result = new NewSurgerySetup()
            {
                Rooms = rooms,
                Specialties = specialties,
                Lateralities = lateralities,
                Surgeons = surgeons,
                Profiles = profiles,
                Proposals = proposals
            };

            return Ok(result);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("newSurgeryVendor")]
        public async Task<ActionResult> GetNewSurgeryVendor()
        {
            var user = await GetUserSecurity();
            

            if (!user.Vendor  && user.RoleType != "Internal")
                return NotFound();
            
            var providers = await _sqlHelper.GetOpFlowSetup();
            var caseProfiles = await _sqlHelper.GetCaseProfiles(user.SelectedLocation);
            var trayGroups = await _sqlHelper.GetTrayGroups(user.SelectedLocation);

            var result = new NewSurgeryVendor()
            {
                Providers = providers,
                CaseProfiles = caseProfiles,
                TrayGroups = trayGroups
            };

            return Ok(result);
        }

        // GET api/surgery?userId=5
        [Route("case")]
        public async Task<ActionResult> GetCase(int caseId)
        {
            var user = await GetUserSecurity();
            

            var schedules = await _sqlHelper.GetCase(caseId, user.SelectedLocation);

            return Ok(schedules);
        }

        // GET api/surgery?userId=5
        [Route("caseNbr/{caseNbr}")]
        public async Task<ActionResult> GetCaseNbr(string caseNbr)
        {
            var user = await GetUserSecurity();
            

            var surgeries = await _sqlHelper.GetScheduledSurgeries(null, null, null, caseNbr, user.SelectedLocation);
            
            return Ok(surgeries);
        }

        [Route("metrics")]
        public async Task<ActionResult> GetSurgeryMetrics(int surgeryId)
        {
            var user = await GetUserSecurity();
            
            
            var surgeryMetrics = await _sqlHelper.GetSurgeryMetrics(surgeryId, user.SelectedLocation);

            var categories = surgeryMetrics.GroupBy(r => r.MetricType)
                .Select(r => new SurgeryMetricCategory()
                {
                    CategoryType = r.Key,
                    Metrics = r.ToList()
                });

            return Ok(
            new {
                Categories = categories
            });
        }

        // GET api/surgery?userId=5
        [Route("searchCases")]
        [HttpPost]
        public async Task<ActionResult> GetCases([FromBody] SearchCasePost post)
        {
            var user = await GetUserSecurity();
            

            var schedules = await _sqlHelper.SearchCases(post.UserID, post.SurgeonUserId, post.RoomGroupId, post.RoomID,
                null, post.ProcedureId, post.SpecialtyId, post.ShowDeleted,
                post.BegDate, post.EndDate, user.SelectedLocation);

            await _sqlHelper.LoadProposalCounts(schedules, user.SelectedLocation);

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

            return Ok(result);
        }

        // GET api/surgery?userId=5
        [Route("searchSurgeonCases")]
        public async Task<ActionResult> GetSurgeonCases(int surgeonUserId, DateTime begDate, DateTime endDate)
        {
            var user = await GetUserSecurity();
            

            var schedules = await _sqlHelper.GetSurgeonCases(surgeonUserId, begDate, endDate, user.SelectedLocation);

            return Ok(schedules);
        }

        // GET api/surgery?userId=5
        [Route("surgeonPreferences")]
        public async Task<ActionResult> GetSurgeonPreferences(int? surgeryId = null, int? userId = null)
        {
            var user = await GetUserSecurity();
            

            var preferences = await _sqlHelper.GetSurgeonPreferences(user.SelectedLocation);

            int? caseProfileId = null;
            var users = new List<int>();
            var surgeonPreferences = new List<SurgeonPreference>();

            if (surgeryId.HasValue)
            {
                var surgery = await _sqlHelper.GetSurgery(surgeryId.Value, user.SelectedLocation);
                var schedules = await _sqlHelper.GetSurgeryUsers(surgeryId.Value, user.SelectedLocation);

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

            return Ok(new {
                SurgeonPreferences = surgeonPreferences
            });
        }

        // GET api/surgery?userId=5
        [Route("surgeonPreference")]
        [HttpPut]
        public async Task<ActionResult> UpdateSurgeonPreferences(int surgeryId, int surgeonPreferenceId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSurgeonPreferences(user.SelectedLocation);
            var preference = result.FirstOrDefault(sp => sp.SurgeonPreferenceID == surgeonPreferenceId);
            if (preference == null)
                return Ok(result);

            var trayGroups = await _sqlHelper.GetTrayGroups(user.SelectedLocation);

            foreach (var trayGroupId in preference.TrayGroupID)
            {
                var trayGroup = trayGroups.FirstOrDefault(tg => tg.TrayGroupID == trayGroupId);
                foreach (var tray in trayGroup.Trays)
                {
                    await _sqlHelper.AddCustomSurgeryItem(surgeryId, tray.TrayItemID, 1, user.SelectedLocation, user.UserID);
                }
            }

            return Ok(result);
        }
        
        // GET api/surgery?userId=5
        [Route("cases")]
        public async Task<ActionResult> GetSurgerySchedule(DateTime? scheduleDate = null, int? roomId = null, string caseNbr = null)
        {
            var user = await GetUserSecurity();
            

            int? userId = null;
            if (roomId == null)
                userId = user.UserID;

            var surgeries = await _sqlHelper.GetScheduledSurgeries(userId, scheduleDate, roomId, caseNbr, user.SelectedLocation);

            foreach (var surgery in surgeries)
            {
                surgery.SurgeryUsers =
                    await _sqlHelper.GetSurgeryUsers(surgery.CaseID, user.SelectedLocation);
            }

            return Ok(surgeries);
        }

        // GET api/surgery?userId=5
        [Route("alerts")]
        public async Task<ActionResult> GetSurgeryAlerts(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var schedules = await _sqlHelper.GetSurgeryAlerts(surgeryId, user.SelectedLocation);

            return Ok(schedules);
        }

        // GET api/values/5
        [Route("procedures")]
        public async Task<ActionResult> GetSurgeryProcedures(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var procedures = await _sqlHelper.GetSurgeryProcedures(surgeryId, user.SelectedLocation);

            return Ok(procedures);
        }

        // GET api/surgery?userId=5
        [Route("delayReasons")]
        public async Task<ActionResult> GetSurgeryDelayReasons()
        {
            var user = await GetUserSecurity();
            

            var reasons = await _sqlHelper.GetSurgeryDelayReasons(user.SelectedLocation);

            return Ok(reasons);
        }

        [Route("users")]
        public async Task<ActionResult> GetSurgeryUsers(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var schedules = await _sqlHelper.GetSurgeryUsers(surgeryId, user.SelectedLocation);

            return Ok(schedules);
        }

        [Route("Vendors")]
        public async Task<ActionResult> GetVendors()
        {
            var user = await GetUserSecurity();


            if (user.RoleType != "Internal")
                return Ok();

            var schedules = await _sqlHelper.GetVendors(user.SelectedLocation);

            return Ok(schedules);
        }

        [Route("VendorReps")]
        public async Task<ActionResult> GetSurgeryVendorReps(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var schedules = await _sqlHelper.GetSurgeryVendorReps(surgeryId, user.SelectedLocation);

            return Ok(schedules);
        }

        // GET api/values/5
        [Route("cards")]
        public async Task<ActionResult> GetSurgeryCardList(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSurgeryCardList(surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("flows")]
        public async Task<ActionResult> GetSurgeryFlowList(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSurgeryFlowList(surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("carditems")]
        public async Task<ActionResult> GetCardItems(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetSurgeryCardItems(surgeryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("cardItemCounts")]
        public async Task<ActionResult> GetSurgeryCardItemCounts(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            var cardItemCounts = await _sqlHelper.GetSurgeryCardItemCounts(surgeryId, user.SelectedLocation);
            var itemCounts = cardItemCounts.CardItemCounts.GroupBy(ic => new { ic.ItemType, ic.TrayName, ic.TrayID });

            var trayOpens = await _sqlHelper.GetSurgeryTrayOpens(surgeryId, user.SelectedLocation);

            var result = new CardItemCountResult()
            {
                Surgery = surgery,
                Collections = new List<TrayCollection>(),
                Trays = new List<TrayUsage>(),
                FlowSteps = await _sqlHelper.GetSurgeryFlowList(surgeryId, user.SelectedLocation),
                Sutures = await _sqlHelper.GetItemSutures(user.SelectedLocation),
                CptCodes = await _sqlHelper.GetSurgeryCPTCodes(surgeryId, user.SelectedLocation),
                SutureCounts = await _sqlHelper.GetSurgerySutureCounts(surgeryId, user.SelectedLocation),
                SupplementalCards = cardItemCounts.SupplementalCards
            };

            foreach (var countType in itemCounts.OrderByDescending(ic => ic.Min(i => i.TrayName)))
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
                            TrayItems = countType.ToList(),
                            SetupAdded = countType.Sum(c => c.SetupAdded ?? 0) > 0
                        });
                        break;
                    case "PROPOSAL":
                        result.ProposedTrays.Add(new TrayUsage()
                        {
                            TrayID = countType.Key.TrayID ?? 0,
                            TrayItems = countType.ToList(),
                            SetupAdded = countType.Sum(c => c.SetupAdded ?? 0) > 0
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
                    tray.OffsiteTray = trayOpen.OffsiteTray;
                    tray.MissingTray = trayOpen.MissingTray;
                    tray.Feedback = trayOpen.Feedback;
                }
            }



            return Ok(result);
        }


        [Route("updateStepEnd")]
        [HttpPost]
        public async Task<ActionResult> UpdateSurgeryStepEndTime(int surgeryId, int stepId, DateTime surgeryEnd)
        {
            var user = await GetUserSecurity();
            

            var steps = await _sqlHelper.GetFlowSurgeryTimings(surgeryId, user.SelectedLocation);
            var step = steps.FirstOrDefault(s => s.StepID == stepId);
            if (step == null) return NotFound();

            foreach (var adjustStep in steps.OrderBy(s => s.StepSequence))
            {
                if (adjustStep.StepSequence < step.StepSequence) continue;

                if (adjustStep.StepSequence == step.StepSequence)
                {
                    adjustStep.EndTime = surgeryEnd.TimeOfDay;
                    continue;
                }

                // move out all subsequent steps as needed
                if (adjustStep.StartTime <= surgeryEnd.TimeOfDay)
                {
                    adjustStep.StartTime = surgeryEnd.TimeOfDay;
                }
                if (adjustStep.EndTime <= surgeryEnd.TimeOfDay)
                {
                    adjustStep.EndTime = surgeryEnd.TimeOfDay;
                }
            }

            await _sqlHelper.UpdateSurgeryFlowTimingOverride(surgeryId, stepId, steps);

            return Ok(surgeryId);

        }

        [Route("updateTrayOpen")]
        public async Task<ActionResult> UpdateSurgeryTrayOpen(int surgeryId, int trayId, bool trayOpened)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateSurgeryTrayOpens(surgeryId, user.SelectedLocation, trayId, trayOpened);

            return Ok(result);

        }

        // GET api/values/5
        [Route("searchScreen")]
        public async Task<ActionResult> GetSearchScreen()
        {
            var user = await GetUserSecurity();
            

            var users = await _sqlHelper.GetSurgeryUsers(user.SelectedLocation);

            var setup = await _sqlHelper.GetOpFlowSetup();
            var location = setup.FirstOrDefault(p => p.Locations.Any(l => l.LocationID == user.SelectedLocation))?
                .Locations.FirstOrDefault(l => l.LocationID == user.SelectedLocation);

            var result = new SearchScreen
            {
                Rooms = await _sqlHelper.GetRooms(user.SelectedLocation),
                RoomGroups = await _sqlHelper.GetRoomGroups(user.SelectedLocation),
                Specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation),
                Users = users,
                Surgeons = users.Where(u => u.RoleID == RoleEnum.Surgeon).ToList(),
                Trays = await _sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation),
                Proposals = await _sqlHelper.GetProposedTrays(null, user.SelectedLocation),
                Bundles = await _sqlHelper.GetBundles(null, user.SelectedLocation),
                Procedures = await _sqlHelper.GetProcedures(null, user.SelectedLocation),
                LocationSetup = location
            };

            return Ok(result);
        }

        // GET api/values/5
        [Route("debriefScreen")]
        public async Task<ActionResult> GetDebriefScreen(int flowId, int? surgeryId)
        {
            var user = await GetUserSecurity();
            

            var flow = await _sqlHelper.GetFlow(flowId, user.SelectedLocation);
            var categories = await _sqlHelper.GetSmartPhraseCategories(user.SelectedLocation);
            var flowPhrases = await _sqlHelper.GetFlowPhrases(flowId, user.SelectedLocation);

            var smartPhrases = new List<SmartPhrase>();
            if (flow != null)
                smartPhrases = await _sqlHelper.GetSmartPhrases(null, null, flow.OwnerUserID, user.SelectedLocation);

            var flowFeedback = await _sqlHelper.GetFlowFeedback(flowId, user.SelectedLocation);
            var surgeonNotes = await _sqlHelper.GetSurgeonNotes(flowId, user.SelectedLocation);
            var flowSteps = await _sqlHelper.GetFlowTimings(flowId, user.SelectedLocation);
            var messages =
                await _sqlHelper.GetMessaging(user.UserID, surgeryId, null, null, user.SelectedLocation);

            var flowImages = await _sqlHelper.GetFlowImages(flowId, user.SelectedLocation);

            var surgeryPhrases = new List<SurgeryPhrase>();
            var surgeryImages = new List<SurgeryImage>();

            if (surgeryId.HasValue)
            {
                surgeryPhrases = await _sqlHelper.GetSurgeryPhrases(surgeryId.Value, user.SelectedLocation);
                surgeryImages = await _sqlHelper.GetSurgeryImages(surgeryId.Value, user.SelectedLocation);
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

            return Ok(result);
        }

        // GET api/values/5
        [Route("trayAudits")]
        public async Task<ActionResult> GetTrayAudits(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var instrumentLookups = await _sqlHelper.GetTrayInstrumentLookups(user.SelectedLocation);
            var audits = await _sqlHelper.GetSurgeryProposedTrays(surgeryId, user.SelectedLocation);
            var groups = await _sqlHelper.GetTrayGroups(user.SelectedLocation);
            var caseProfile = await _sqlHelper.GetSurgeryCaseProfile(surgeryId, user.SelectedLocation);

            return Ok(new
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
        [Route("roomSummary", Name = "GetSurgeryRoomSummary")]
        public async Task<ActionResult> GetSurgeryRoomSummary(int surgeryId)
        {
            var user = await GetUserSecurity();
            
            var userObject = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            var result = await _sqlHelper.GetSurgeryRoomSummary(surgeryId, user.SelectedLocation);

            foreach (var surgery in result)
            {
                //surgery.Patient = await secureSqlHelper.GetPatient(surgery.PatientID,
                //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

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

            return Ok(result);
        }

        // POST api/values
        [Route("roomOverview", Name = "GetSurgeryRoomOverview")]
        [HttpPost]
        public async Task<ActionResult> GetSurgeryRoomOverview([FromBody] RoomOverviewPost post)
        {
            var user = await GetUserSecurity();
            
            var userObject = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            var surgeries = await _sqlHelper.GetSurgeryRoomOverview(
                post.SpecialtyId, post.RoomGroupId, post.RoomId, post.SurgeonId,
                post.SurgeryDate, user.SelectedLocation);

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
                //surgery.Patient = await secureSqlHelper.GetPatient(surgery.PatientID,
                //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

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

            return Ok(result);
        }

        // POST api/values
        [Route("estCompTime/{surgeryId}", Name = "UpdateEstCompTime")]
        [HttpPost]
        public async Task<ActionResult> UpdateEstCompTime(int surgeryId, DateTime estCompTime)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryEstCompTime(surgeryId, estCompTime, user.SelectedLocation);

            return await GetSurgeryCardItemCounts(surgeryId);
        }

        // POST api/values
        [Route("trayFeedback", Name = "UpdateTrayFeedback")]
        [HttpPost]
        public async Task<ActionResult> UpdateTrayFeedback(int surgeryId, int trayId, [FromBody] SurgeryTrayFeedback post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryTrayFeedback(surgeryId, trayId, post.Feedback, user.SelectedLocation);

            return await GetSurgeryCardItemCounts(surgeryId);
        }

        // POST api/values
        [Route("debrief", Name = "UpdateDebrief")]
        [HttpPut]
        public async Task<ActionResult> UpdateDebrief(int surgeryPhraseId, int stepId, int roleId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.SurgeryPhraseDebrief(surgeryPhraseId, stepId, roleId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("caseNotes", Name = "UpdateCaseNotes")]
        [HttpPut]
        public async Task<ActionResult> UpdateCaseNotes(int surgeryId, [FromBody]DebriefUpdatePost update)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.SurgeryUpdateCaseNotes(surgeryId, update.CaseNotes, user.SelectedLocation);

            return Ok();
        }

        [Route("perioperativeCaseProfile")]
        [HttpPost]
        public async Task<ActionResult> UpdatePerioperativeCaseProfile(int surgeryId, [FromBody] CaseProfileSchedulePost post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryPerioperativeCaseProfile(surgeryId, post.Questions, user.SelectedLocation);

            return Ok(surgeryId);
        }

        // POST api/values
        [Route("surgeryPhrase", Name = "AddSurgerySmartPhrase")]
        [HttpPost]
        public async Task<ActionResult> AddSurgerySmartPhrase(int surgeryId, int smartPhraseId)
        {
            var user = await GetUserSecurity();
            

            try
            {
                await _sqlHelper.AddSurgerySmartPhrase(surgeryId, smartPhraseId,
                    user.SelectedLocation);
            }
            catch (Microsoft.Data.SqlClient.SqlException sqlEx)
            {
                if (sqlEx.Message.ToUpper().Contains("UNIQUE"))
                {
                    return Conflict(
                        "Cannot assign instruction to step multiple times");
                }

                throw;
            }

            return Ok();
        }

        // POST api/values
        [Route("surgeryPhrase", Name = "UpdateSurgeryPhrase")]
        [HttpPut]
        public async Task<ActionResult> UpdateSurgeryPhrase(int surgeryId, int surgeryPhraseId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryPhrase(surgeryId, surgeryPhraseId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("surgeryPhrase", Name = "DeleteSurgeryPhrase")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSurgeryPhrase(int surgeryId, int smartPhraseId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeryPhrase(surgeryId, smartPhraseId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("surgeonNote", Name = "NewSurgeonNote")]
        [HttpPost]
        public async Task<ActionResult> NewSurgeonNote([FromBody]SurgeonNotePost smartPhrase)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.NewSurgeonNote(smartPhrase.Phrase, smartPhrase.FlowID, smartPhrase.StepID, smartPhrase.RoleID,
                user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("surgeonNote", Name = "UpdateSurgeonNote")]
        [HttpPut]
        public async Task<ActionResult> UpdateSurgeonNote(int surgeonNoteId, [FromBody]PhraseUpdatePost debriefUpdate)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeonNote(surgeonNoteId,
                debriefUpdate.Comments, debriefUpdate.StepID, debriefUpdate.RoleID, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("surgeonNote", Name = "DeleteSurgeonNote")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSurgeonNote(int surgeonNoteId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeonNote(surgeonNoteId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("assignCard", Name = "AssignCard")]
        public async Task<ActionResult> AssignToCard(int surgeryId, int cardId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AssignCardToCase(cardId, surgeryId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("assignRoomSetup", Name = "AssignRoomSetupCase")]
        public async Task<ActionResult> AssignRoomSetupToCase(int surgeryId, int roomSetupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AssignRoomSetupToCase(roomSetupId, surgeryId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("assignFlow", Name = "AssignFlowCase")]
        public async Task<ActionResult> AssignToFlowCase(int surgeryId, int flowId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AssignFlowToCase(flowId, surgeryId, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        public async Task<ActionResult> Post([FromBody]SurgeryPost surgery)
        {
            try
            {
                var secureUser = await GetUserSecurity();
                
                if (!secureUser.Vendor)
                    surgery.VendorID = null;

                var user = await _sqlHelper.GetUser(secureUser.SelectedLocation, secureUser.UserID);

                //var patientId = await secureSqlHelper.CreatePatient(surgery.PtAcctNbr,
                //    surgery.PtDOB, surgery.PtGender, surgery.PtFirstName, surgery.PtLastName, surgery.PtMiddleInitial, surgery.PtBMI,
                //    user.UserID, user.FirstName, user.LastName, (int)user.RoleID);
                int? patientId = null;

                var caseId = await _sqlHelper.CreateCase(patientId, secureUser.UserID, surgery.SpecialtyID,
                    secureUser.SelectedLocation, surgery.CaseNbr);

                var cardFlowRoom = (surgery.BundleID.HasValue && surgery.SurgeonUserID.HasValue) ?
                    await _sqlHelper.GetBundleDefaultCardFlowRoom(surgery.BundleID.Value, surgery.SurgeonUserID.Value, secureUser.SelectedLocation) :
                    (await _sqlHelper.GetProcedureDefaultCardFlowRoom(surgery.CptCode, secureUser.SelectedLocation)).FirstOrDefault();

                var surgeryId = await _sqlHelper.CreateSurgery(surgery, patientId, caseId,
                    surgery.CardID ?? cardFlowRoom?.CardID, cardFlowRoom?.TemplateFlowID, cardFlowRoom?.TemplateRoomSetupID,
                    secureUser.SelectedLocation);

                if (surgery.SecondarySurgeons != null && surgery.SecondarySurgeons.Any())
                {
                    foreach (var userId in surgery.SecondarySurgeons)
                    {
                        await _sqlHelper.AddSurgeryUser(surgeryId, userId, secureUser.SelectedLocation);
                    }
                }

                if (surgery.TrayGroupID != null && surgery.TrayGroupID.Any())
                {
                    var trayGroups = await _sqlHelper.GetTrayGroups(secureUser.SelectedLocation);

                    foreach (var trayGroup in trayGroups.Where(t => surgery.TrayGroupID.Any(stg => t.TrayGroupID == stg)))
                    {
                        foreach (var tray in trayGroup.Trays)
                        {
                            await _sqlHelper.AddCustomSurgeryItem(surgeryId, tray.TrayItemID, 1, secureUser.SelectedLocation, user.UserID);
                        }
                    }
                }

                if (surgery.TrayProposalID != null)
                {
                    await _sqlHelper.UpdateProposedTrayCount(surgery.TrayProposalID.Value, surgeryId, null, null, secureUser.SelectedLocation);
                }

                return CreatedAtAction("Post", surgeryId);
            }
            catch(Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        // POST api/values
        [HttpDelete]
        [Route("customSurgeryItem", Name = "RemoveCustomSurgeryItem")]
        public async Task<ActionResult> RemoveCustomSurgeryItem(int surgeryId, int itemId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeryCustomItem(surgeryId, itemId, user.SelectedLocation);
            
            return Ok(surgeryId);
        }

        // POST api/values
        [HttpPost]
        [Route("addSurgeryItemUse", Name = "AddCustomSurgeryItem")]
        public async Task<ActionResult> AddCustomSurgeryItem(int surgeryId, [FromBody]SurgeryCustomItemPost post)
        {
            var user = await GetUserSecurity();
            

            foreach (var customItem in post.Items)
            {
                if (customItem.TrayID.HasValue)
                {
                    await _sqlHelper.AddCustomSurgeryTrayItem(surgeryId, customItem.TrayID ?? 0, customItem.ItemID, customItem.Quantity, user.SelectedLocation, user.UserID);
                }
                else
                {
                    await _sqlHelper.AddCustomSurgeryItem(surgeryId, customItem.ItemID, customItem.Quantity, user.SelectedLocation, user.UserID);
                }
            }
        
            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("addSurgeryMudBox", Name = "AddSurgeryMudBox")]
        public async Task<ActionResult> AddSurgeryMudBox(int surgeryId, int instrumentId, int trayId)
        {
            var user = await GetUserSecurity();
            

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            var cards = await _sqlHelper.GetCardData(surgery?.CardID ?? -1, user.SelectedLocation);
            var card = cards.FirstOrDefault();

            if (card == null) return NotFound();

            var opp = await _sqlHelper.GetProcedureProfileByCard(card.CardID, user.SelectedLocation);
            var surgeon = await _sqlHelper.GetUser(user.SelectedLocation, surgery.UserID);

            var boxName = opp?.ProcedureProfileName ?? card.CardDescription;

            var items = await _sqlHelper.GetSurgeryCardItems(surgeryId, user.SelectedLocation);
            var trayItems = await _sqlHelper.GetTrayItems(trayId, user.SelectedLocation);

            var quantity = trayItems.First(ti => ti.InstrumentID == instrumentId).Quantity;

            var trayName = $"MB {boxName} {surgeon.LastName}";
            var item = items.FirstOrDefault(i => i.ItemDescription == trayName);

            var mudBoxId = item?.ItemID;

            if (mudBoxId == null)
            {
                mudBoxId = await _sqlHelper.AddSurgeryMudBox(surgeryId, trayName, user.SelectedLocation);
            }

            await _sqlHelper.InsertTrayInstrument(mudBoxId.Value, instrumentId, quantity, user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("addTemporaryTray", Name = "AddTemporaryTray")]
        public async Task<ActionResult> AddTemporaryTray(int surgeryId, string trayName)
        {
            var user = await GetUserSecurity();
            

            trayName = $"SURG {surgeryId} TEMP: {trayName ?? string.Empty}";

            await _sqlHelper.AddSurgeryTemporaryTray(surgeryId, trayName, user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("addProposedTray", Name = "AddProposedTray")]
        public async Task<ActionResult> AddProposedTray(int surgeryId, int trayProposalId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddSurgeryProposedTray(surgeryId, trayProposalId, user.SelectedLocation);
            
            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("addProposedTrayGroup", Name = "AddProposedTrayGroup")]
        public async Task<ActionResult> AddProposedTrayGroup(int surgeryId, int trayGroupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddSurgeryProposedTrayGroup(surgeryId, trayGroupId, user.SelectedLocation);

            return Ok(0);
        }

        [Route("updateTeams")]
        [HttpPost]
        public async Task<ActionResult> UpdateTeams([FromBody]SurgeryTeamUpdate teamUpdate)
        {
            var user = await GetUserSecurity();
            

            foreach (var surgeryId in teamUpdate.Surgeries)
            {
                foreach (var edit in teamUpdate.Edits)
                {
                    if (edit.Assign)
                    {
                        await _sqlHelper.AddSurgeryUser(surgeryId, edit.UserID, user.SelectedLocation);
                    }
                    else
                    {
                        await _sqlHelper.DeleteSurgeryUser(surgeryId, edit.UserID, user.SelectedLocation);
                    }
                }
            }

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("user", Name = "AddSurgeryUser")]
        public async Task<ActionResult> AddSurgeryUser(int surgeryId, int userId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddSurgeryUser(surgeryId, userId, user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("notifyAdministrator", Name = "NotifyAdministrator")]
        public async Task<ActionResult> NotifyAdministrator(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null)
                return NotFound(0);

            var helpString = "Help needed with surgery in room: " + surgery.RoomDescription;
            var administrator = "16127016239";

            //SmsNotification.NotifyUser(administrator, helpString);

            return Ok(0);
        }

        // POST api/values
        [HttpDelete]
        [Route("user", Name = "DeleteSurgeryUser")]
        public async Task<ActionResult> DeleteSurgeryUser(int surgeryId, int userId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeryUser(surgeryId, userId, user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("surgeryProcedure", Name = "AddSurgeryProcedure")]
        public async Task<ActionResult> AddSurgeryProcedure(int surgeryId, int procedureId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddSurgeryProcedure(surgeryId, procedureId, user.SelectedLocation);
        
            return Ok(0);
        }

        // POST api/values
        [HttpPut]
        [Route("surgeryProcedure", Name = "UpdateSurgeryProcedure")]
        public async Task<ActionResult> UpdateSurgeryProcedure(int surgeryId, string cptCode, [FromBody]SurgeryProcedureEditPost procedureEdit)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryProcedure(surgeryId, cptCode, procedureEdit.IsPerformed ? "Y" : "N", user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpDelete]
        [Route("surgeryProcedure", Name = "DeleteSurgeryProcedure")]
        public async Task<ActionResult> DeleteSurgeryProcedure(int surgeryId, string cptCode)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeryProcedure(surgeryId, cptCode, user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("surgeryCard", Name = "AddSurgeryCard")]
        public async Task<ActionResult> AddSurgeryCard(int surgeryId, int cardId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AddSurgeryCard(surgeryId, cardId, user.SelectedLocation, user.UserID);

            return Ok(0);
        }

        // POST api/values
        [HttpDelete]
        [Route("surgeryCard", Name = "DeleteSurgeryCard")]
        public async Task<ActionResult> DeleteSurgeryCard(int surgeryId, int cardId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSurgeryCard(surgeryId, cardId, user.SelectedLocation, user.UserID);

            return Ok(0);
        }

        // POST api/values
        [Route("surgeryImage", Name = "NewSurgeryImage")]
        [HttpPut]
        public async Task<ActionResult> NewSurgeryImage(IFormFile file, int surgeryId, int stepId, int roleId, string comment)
        {
            var user = await GetUserSecurity();

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null) return NotFound();


            // extract file name and file contents
            var fileName = file.FileName;
            var fileExtension = Path.GetExtension(fileName);
            byte[] fileContents;
            using (var stream = file.OpenReadStream())
            using (var memStream = new MemoryStream())
            {
                stream.CopyTo(memStream);

                fileContents = memStream.ToArray();
            }


            if (fileExtension == ".png" ||
                fileExtension == ".jpg" ||
                fileExtension == ".jpeg")
            {
                fileContents = BlobStorageHelper.CompressImage(fileContents);
            }

            var surgeryImageId = await _sqlHelper.NewSurgeryImage(surgeryId, stepId, roleId, comment,
                user.SelectedLocation);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            await _blobStorageHelper.PutBlobBytes(folder, surgeryImageId.ToString(), fileContents);

            return Ok();
        }

        // POST api/values
        [Route("surgeryImage", Name = "UpdateSurgeryImage")]
        [HttpPost]
        public async Task<ActionResult> UpdateSurgeryImage(int surgeryImageId, [FromBody]FlowImagePost surgeryImage)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryImage(surgeryImageId, surgeryImage.Comment, surgeryImage.StepID, surgeryImage.RoleID, user.SelectedLocation);

            return Ok();
        }

        // POST api/values
        [Route("api/flow/rotateSurgeryImage", Name = "RotateSurgeryImage")]
        [HttpPut]
        public async Task<ActionResult> RotateSurgeryImage(int surgeryImageId, int surgeryId, int direction)
        {
            var user = await GetUserSecurity();
            

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null) return NotFound();

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            await _blobStorageHelper.RotateImage(folder, surgeryImageId.ToString(), direction);

            return Ok();
        }

        // POST api/values
        [Route("surgeryImage", Name = "DeleteSurgeryImage")]
        [HttpDelete]
        public async Task<ActionResult> DeleteSurgeryImage(int surgeryImageId, int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            if (surgery == null) return NotFound();

            await _sqlHelper.DeleteSurgeryImage(surgeryImageId, user.SelectedLocation);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            await _blobStorageHelper.DeleteBlob(folder, surgeryImageId.ToString());

            return Ok();
        }

        // POST api/values
        [HttpPost]
        [Route("updateCountsUsages", Name = "UpdateSurgeryCountsUsages")]
        public async Task<ActionResult> UpdateSurgeryCountsUsages(int surgeryId, [FromBody]SurgeryCountSummaryPost post)
        {
            var user = await GetUserSecurity();
            

            try
            {
                await _sqlHelper.UpdateSurgeryCount(surgeryId, post.ItemCounts, post.CountComments, post.SurgeryType, user.SelectedLocation, user.UserID);
                await _sqlHelper.UpdateSurgeryInstrumentCount(surgeryId, post.InstrumentCounts, user.SelectedLocation, user.UserID);
                await _sqlHelper.UpdateSurgeryProposedCount(surgeryId, post.ProposedCounts, user.SelectedLocation, user.UserID);
                await _sqlHelper.UpdateSurgerySutureCount(surgeryId, post.SutureCounts, post.DeletedSutures, user.SelectedLocation, user.UserID);
                await _sqlHelper.UpdateSurgeryMetricAnswers(surgeryId, post.MetricAnswers, user.SelectedLocation, user.UserID);

                if (post?.Answers.Any() == true)
                {
                    await _sqlHelper.UpdateSurgeryQuestionAnswers(surgeryId, post.Answers, user.SelectedLocation);
                }

                await _sqlHelper.UpdateSurgeryCPTs(surgeryId, post.SurgeryCpts, user.SelectedLocation);
            }
            catch (Exception e)
            {
                LogHelper.LogException(e);
                throw;
            }

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("updateCounts", Name = "UpdateSurgeryCounts")]
        public async Task<ActionResult> UpdateSurgeryCounts(int surgeryId, int sharpCount, int needleCount, int lapCount, int specimenCount)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryHeaderCounts(surgeryId, sharpCount, needleCount, lapCount, specimenCount, user.SelectedLocation);
            
            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("updateStaffChange", Name = "UpdateStaffChange")]
        public async Task<ActionResult> UpdateStaffChange(int surgeryId, string staffChange)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateStaffChange(surgeryId, staffChange, user.SelectedLocation);

            return Ok(0);
        }

        // POST api/values
        [HttpPost]
        [Route("reviewComplete", Name = "SurgeryReviewComplete")]
        public async Task<ActionResult> SurgeryReviewComplete(int surgeryId, DateTime reviewComplete)
        {
            var user = await GetUserSecurity();
            

            var success = await _sqlHelper.SurgeryReviewComplete(surgeryId, reviewComplete, user.UserID, user.SelectedLocation);

            return Ok(success);
        }

        // POST api/values
        [HttpPost]
        [Route("editProperties", Name = "EditSurgeryProperties")]
        public async Task<ActionResult> EditSurgeryProperties(int surgeryId, [FromBody]SurgeryEditPost surgeryEditPost)
        {
            var user = await GetUserSecurity();
            
            var success = await _sqlHelper.SurgeryEditProperties(surgeryId,
                surgeryEditPost.RoomID, surgeryEditPost.ScheduleDateTime, user.SelectedLocation);

            if (user.RoleType == "Internal")
                await _sqlHelper.SurgeryEditVendor(surgeryId, surgeryEditPost.VendorID, user.SelectedLocation);

            if (!surgeryEditPost.NotificationUser.HasValue) return Ok(success);


            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);
            var userObject = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

            
            var message = $"Surgery #{surgery.CaseNumber} patient {"PATIENT"} room {surgery.RoomDescription} {surgery.ScheduleTime:hh\\:mm} modified - please review schedule";

            var notificationUser = await _sqlHelper.GetUser(user.SelectedLocation,  surgeryEditPost.NotificationUser.Value);
            //if (notificationUser?.CellPhone != null)
            //    SmsNotification.NotifyUser(notificationUser.CellPhone, message);

            return Ok(success);
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [Route("{surgeryId}")]
        public async Task<ActionResult> Delete(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var success = await _sqlHelper.DeleteSurgery(surgeryId, user.SelectedLocation);

            return Ok(success);
        }
    }
}