using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using System.Text.Json;
using NPOI.OpenXmlFormats.Dml.Diagram;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/trayproposal")]
    public class TrayProposalController : OpFlowController
    {
        private BlobStorageHelper _blobStorageHelper;
        private SqlHelper _readOnlySqlHelper;

        public TrayProposalController(
            BlobStorageHelper blobStorageHelper,
            IConfiguration configuration,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
            _blobStorageHelper = blobStorageHelper;
            _readOnlySqlHelper = new SqlHelper(configuration, "CommonReadOnlyConnection");
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("trayRationalization")]
        public async Task<ActionResult> GetTrayRationalization()
        {
            var user = await GetUserSecurity();

            var specialties = await _readOnlySqlHelper.GetSpecialties(user.SelectedLocation);
            var users = await _readOnlySqlHelper.SearchUsers(null, null, null, user.SelectedLocation);
            var instrumentLookups = await _readOnlySqlHelper.GetTrayInstrumentLookups(user.SelectedLocation);
            var trays = await _readOnlySqlHelper.GetItems("tray", null, null, user.SelectedLocation);
            var collections = await _readOnlySqlHelper.GetItems("collection", null, null, user.SelectedLocation);
            var proposedTrays = await _readOnlySqlHelper.GetProposedTrays(null, user.SelectedLocation);
            var baselineTrays = await _readOnlySqlHelper.GetBaselineTrays(user.SelectedLocation);
            var schedules = await _readOnlySqlHelper.GetProposedTraySchedule(null, null, user.UserID, user.SelectedLocation);
            var vendors = await _readOnlySqlHelper.GetVendors(user.LocationID);
            var questions = await _readOnlySqlHelper.GetTrayQuestions(null, user.SelectedLocation);
            var phases = await _readOnlySqlHelper.GetTrayProposalPhases(user.SelectedLocation);
            var cardCategories = await _readOnlySqlHelper.GetCardCategories();
            var procedureProfiles = await _readOnlySqlHelper.GetProcedureProfiles();
            var trayGroups = await _readOnlySqlHelper.GetTrayGroups(user.SelectedLocation);
            var proposalCardCategories = await _readOnlySqlHelper.GetProposedTrayCardCategories(user.SelectedLocation);
            var caseProfiles = await _readOnlySqlHelper.GetCaseProfiles(user.SelectedLocation);
            var surgeonPreferences = await _readOnlySqlHelper.GetSurgeonPreferences(user.SelectedLocation);
            var communicationMethods = await _readOnlySqlHelper.GetTrayCommunicationMethods(user.SelectedLocation);
            var rules = await _readOnlySqlHelper.GetProposedTrayScheduleRules(user.UserID, user.SelectedLocation);
            var roomGroups = await _readOnlySqlHelper.GetRoomGroups(user.SelectedLocation);
            var orgCharts = await _readOnlySqlHelper.GetOrgChartAttachments(user.SelectedLocation);

            var attachments = await _readOnlySqlHelper.GetImplementationAttachments(user.SelectedLocation);
            var implementation = TrayImplementation.GetImplementationSteps(attachments);

            schedules = ApplyRules(schedules, rules);

            if (user.Vendor)
            {
                vendors = vendors.Where(v => v.VendorID == user.ProviderID).ToList();
            }

            foreach (var surgeonPreference in surgeonPreferences)
            {
                var trayGroupString = trayGroups.Where(tg => surgeonPreference.TrayGroupID?.Contains(tg.TrayGroupID ?? -1) == true).Select(tg => tg.GroupName);
                surgeonPreference.TrayGroup = string.Join(",", trayGroupString);
            }

            foreach (var proposal in proposedTrays)
            {
                proposal.CardCategories = proposalCardCategories.Where(p => p.TrayProposalID == proposal.TrayProposalID).ToList();
            }

            var result = new
            {
                Specialties = specialties,
                Surgeons = users.Where(u => u.RoleID == RoleEnum.Surgeon),
                Users = users,
                Categories = instrumentLookups.Categories,
                Eponyms = instrumentLookups.Eponyms,
                Types = instrumentLookups.Types,
                CardCategories = cardCategories,
                ProcedureProfiles = procedureProfiles,
                TrayGroups = trayGroups,
                Trays = trays,
                Schedules = schedules,
                Collections = collections,
                Proposals = proposedTrays,
                Vendors = vendors,
                BaselineTrays = baselineTrays,
                Questions = questions,
                Phases = phases,
                CaseProfiles = caseProfiles,
                SurgeonPreferences = surgeonPreferences,
                Rules = rules,
                Implementation = implementation,
                OrgCharts = orgCharts,
                RoomGroups = roomGroups,
                CommunicationMethods = communicationMethods
            };


            return Ok(result);
        }

        [Route("homeDashboard")]
        public async Task<ActionResult> GetHomeDashboard()
        {
            var user = await GetUserSecurity();
            

            var proposedTrays = await _sqlHelper.GetProposedTrayDashboard(user.SelectedLocation);
            
            var result = new
            {
                Proposals = proposedTrays
            };


            return Ok(result);
        }

        private static List<TrayProposalSchedule> ApplyRules(IEnumerable<TrayProposalSchedule> schedule, TrayProposalScheduleRule rules)
        {
            if (rules.Surgeon != null)
            {
                var surgeonList = JsonSerializer.Deserialize<List<int>>(rules.Surgeon);

                if (surgeonList?.Any() == true)
                {
                    schedule = schedule.Where(s => s.SurgeryUsers.Any(su => surgeonList.Contains(su.UserID)));
                }
            }

            if (rules.Category != null)
            {
                var categoryList = JsonSerializer.Deserialize<List<int>>(rules.Category);

                if (categoryList?.Any() == true)
                {
                    schedule = schedule.Where(s => s.CardCategories.Any(cc => categoryList.Contains(cc.CardCategoryID)));
                }
            }

            if (rules.StartDate != null)
            {
                schedule = schedule.Where(s => s.ScheduleTime >= rules.StartDate);
            }

            if (rules.EndDate != null)
            {
                schedule = schedule.Where(s => s.ScheduleTime <= rules.EndDate);
            }

            return schedule.ToList();
        }

        [Route("proposedTray")]
        [HttpGet]
        public async Task<ActionResult> GetProposedTrays()
        {
            var user = await GetUserSecurity();
            
            var trayId = await _readOnlySqlHelper.GetProposedTrays(null, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("proposedTray/{trayProposalId}")]
        [HttpGet]
        public async Task<ActionResult> GetProposedTray(int trayProposalId)
        {
            var user = await GetUserSecurity();

            var proposedTray = (await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).First();
            var instruments = await _readOnlySqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
            var documents = await _readOnlySqlHelper.GetProposedTrayApprovalDocuments(trayProposalId, user.SelectedLocation);
            var audits = await _readOnlySqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.SelectedLocation);
            var counts = await _readOnlySqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.SelectedLocation);
            var trayCounts = await _readOnlySqlHelper.GetTrayCountSummary(trayProposalId, user.SelectedLocation);
            var sourceTrays = await _readOnlySqlHelper.GetSourceTraySummary(trayProposalId, user.SelectedLocation);
            var cardCategories = await _readOnlySqlHelper.GetProposedTrayCardCategories(trayProposalId, user.SelectedLocation);
            var history = await _readOnlySqlHelper.GetProposedTrayLog(trayProposalId, user.SelectedLocation);

            proposedTray.InstrumentCount = instruments.Sum(i => i.Quantity);
            foreach (var sourceTray in sourceTrays)
            {
                sourceTray.InstrumentCount = sourceTray.Instruments.Sum(i => i.Quantity) * sourceTray.InstanceCount;
                sourceTray.ProposedInstrumentCount = instruments.Sum(i => i.Quantity) * sourceTray.ProposedInstanceCount;
            }

            return Ok(new
            {
                ReadOnly = (user.Vendor && proposedTray?.VendorID != user.ProviderID && user.RoleType != "Internal"),
                ProposedTray = proposedTray,
                Instruments = instruments,
                ApprovalDocuments = documents,
                Audits = audits,
                Counts = counts,
                CardCategories = cardCategories,
                TrayCounts = trayCounts,
                ApprovalAudits = audits.Where(a => a.AuditUserID.HasValue).OrderBy(a => a.SurgeonName).ToList(),
                ApprovalCounts = counts.Where(c => c.AuditUserID.HasValue).OrderBy(c => c.SurgeonName).ToList(),
                SourceTrays = sourceTrays,
                History = history
            });
        }

        [Route("statusLog/{trayProposalId}")]
        [HttpGet]
        public async Task<ActionResult> GetStatusLog(int trayProposalId)
        {
            var user = await GetUserSecurity();
            

            var statusLog = await _sqlHelper.GetProposedTrayStatusLog(trayProposalId, user.SelectedLocation);

            return Ok(statusLog);
        }

        [Route("cardOverlap/{trayProposalId}")]
        [HttpGet]
        public async Task<ActionResult> GetCardOverlap(int trayProposalId, string sortBy = null, int? overlapPcnt = 0)
        {
            var user = await GetUserSecurity();
            
            
            var cardOverlaps = await _sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.SelectedLocation);

            switch (sortBy)
            {
                case "specialty":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.SpecialtyName).ToList() : cardOverlaps.OrderByDescending(c => c.SpecialtyName).ToList();
                    break;
                case "card":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.CardDescription).ToList() : cardOverlaps.OrderByDescending(c => c.CardDescription).ToList();
                    break;
                case "surgeon":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.SurgeonName).ToList() : cardOverlaps.OrderByDescending(c => c.SurgeonName).ToList();
                    break;
                case "tray":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.TrayName).ToList() : cardOverlaps.OrderByDescending(c => c.TrayName).ToList();
                    break;
                case "overlapPcnt":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.OverlapPcnt).ToList() : cardOverlaps.OrderByDescending(c => c.OverlapPcnt).ToList();
                    break;
                case "audit":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.AuditsComplete).ToList() : cardOverlaps.OrderByDescending(c => c.AuditsComplete).ToList();
                    break;
                case "count":
                    cardOverlaps = sortBy == "asc" ? cardOverlaps.OrderBy(c => c.TimesUsed).ToList() : cardOverlaps.OrderByDescending(c => c.TimesUsed).ToList();
                    break;
            }

            return Ok(new
            {
                Cards = cardOverlaps.Where(i => i.OverlapPcnt >= (overlapPcnt ?? 0))
            });
        }

        [Route("proposalNotes/{trayProposalId}")]
        [HttpPost]
        public async Task<ActionResult> UpdateProposalNotes(int trayProposalId, [FromBody] ProposedTrayNotesPost request)
        {
            var user = await GetUserSecurity();
            

            var cases = await _sqlHelper.UpdateProposedTrayComments(trayProposalId, request.Notes, user.SelectedLocation);

            return Ok(trayProposalId);
        }

        [Route("searchCases")]
        [HttpGet]
        public async Task<ActionResult> SearchCases(string target,
            int? trayProposalId = null, int? surgeonUserId = null, int? specialtyId = null, int? trayId = null, int? cardId = null, int? roomGroupId = null,
            string priority = null, DateTime? beginDate = null, DateTime? endDate = null)
        {
            var user = await GetUserSecurity();
            

            beginDate = beginDate ?? (DateTime.Today.AddDays(-1));

            var cases = await _sqlHelper.GetProposedTrayAuditSearch(trayProposalId, surgeonUserId, specialtyId, trayId, cardId, priority, roomGroupId,
                beginDate, endDate, 
                target, user.SelectedLocation);

            return Ok(cases);
        }

        [HttpPut]
        [Route("trayApproval/{trayProposalId}/{typeId}", Name = "PutTrayApproval")]
        public async Task<ActionResult> PutTrayApproval(IFormFile file, int trayProposalId, int typeId)
        {
            var user = await GetUserSecurity();
                        
            // validate location auth
            var proposedTray = (await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).First();
            if (proposedTray == null)
                return NotFound();

            try
            {
                // extract file name and file contents
                var fileName = file.FileName;
                byte[] fileContents;
                using (var stream = file.OpenReadStream())
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);

                    fileContents = memStream.ToArray();
                }

                await _sqlHelper.UpdateProposedTrayApproval(trayProposalId, fileName, typeId, user.SelectedLocation);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ApprovalImages, trayProposalId);

                

                await _blobStorageHelper.PutBlobBytes(folder, typeId.ToString(), fileContents);

                return Ok(200);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        [HttpPut]
        [Route("orgChart", Name = "PutOrgChart")]
        public async Task<ActionResult> PutOrgChart(IFormFile file, string type, int? tray)
        {
            var user = await GetUserSecurity();

            try
            {
                // extract file name and file contents
                var fileName = file.FileName;
                byte[] fileContents;
                using (var stream = file.OpenReadStream())
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);

                    fileContents = memStream.ToArray();
                }

                var orgChartId = await _sqlHelper.UpdateOrgChartAttachment(type, tray, fileName, user.SelectedLocation);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, tray ?? -1);

                

                await _blobStorageHelper.PutBlobBytes(folder, orgChartId.ToString(), fileContents);

                var proposals = await _readOnlySqlHelper.GetProposedTrays(null, user.SelectedLocation); 
                var orgCharts = await _readOnlySqlHelper.GetOrgChartAttachments(user.SelectedLocation);

                return Ok(new
                {
                    Proposals = proposals,
                    OrgCharts = orgCharts
                });
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        [HttpDelete]
        [Route("orgChart", Name = "DeleteOrgChart")]
        public async Task<ActionResult> DeleteOrgChart(string type, int? id)
        {
            var user = await GetUserSecurity();

            try
            {
                

                if (type == "T")
                {
                    var proposalChecks = await _readOnlySqlHelper.GetProposedTrays(id, user.SelectedLocation);
                    var proposal = proposalChecks.First();

                    await _sqlHelper.DeleteProposedTrayOrgChart(null, id, user.SelectedLocation);

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, id ?? -1);
                    await _blobStorageHelper.DeleteBlob(folder, id.ToString());
                }
                else
                {
                    var attachments = await _readOnlySqlHelper.GetOrgChartAttachments(user.SelectedLocation);
                    var attachment = attachments.First(a => a.OrgChartID == id);

                    await _sqlHelper.DeleteProposedTrayOrgChart(id, null, user.SelectedLocation);

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, -1);
                    await _blobStorageHelper.DeleteBlob(folder, id.ToString());
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }

            var proposals = await _readOnlySqlHelper.GetProposedTrays(null, user.SelectedLocation);
            var orgCharts = await _readOnlySqlHelper.GetOrgChartAttachments(user.SelectedLocation);

            return Ok(new
            {
                Proposals = proposals,
                OrgCharts = orgCharts
            });
        }

        [HttpPut]
        [Route("implementation", Name = "PutImplementation")]
        public async Task<ActionResult> PutImplementation(IFormFile file, int target)
        {
            var user = await GetUserSecurity();

            try
            {
                // extract file name and file contents
                var fileName = file.FileName;
                byte[] fileContents;
                using (var stream = file.OpenReadStream())
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);

                    fileContents = memStream.ToArray();
                }

                var attachmentId = await _sqlHelper.UpdateImplementationAttachment(target, fileName, user.SelectedLocation);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ImplementationImages, target);

                

                await _blobStorageHelper.PutBlobBytes(folder, attachmentId.ToString(), fileContents);

                var attachments = await _readOnlySqlHelper.GetImplementationAttachments(user.SelectedLocation);
                var result = TrayImplementation.GetImplementationSteps(attachments);

                return Ok(result);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        [HttpGet]
        [Route("implementationAttachment", Name = "GetImplementationAttachment")]
        public async Task<ActionResult> GetImplementationAttachment(int attachmentId)
        {
            var user = await GetUserSecurity();

            try
            {
                var attachments = await _readOnlySqlHelper.GetImplementationAttachments(user.SelectedLocation);
                var attachment = attachments.First(a => a.AttachmentID == attachmentId);

                if (attachment != null)
                {
                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ImplementationImages, attachment.Target);

                    

                    var binary = await _blobStorageHelper.GetBlobBytes(folder, attachmentId.ToString());
                    
                    return File(binary, "application/octet-stream", attachment.Filename);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }

            return NotFound();
        }

        [HttpDelete]
        [Route("implementationAttachment", Name = "DeleteImplementationAttachment")]
        public async Task<ActionResult> DeleteImplementationAttachment(int attachmentId)
        {
            var user = await GetUserSecurity();

            var attachments = await _readOnlySqlHelper.GetImplementationAttachments(user.SelectedLocation);
            var attachment = attachments.FirstOrDefault(a => a.AttachmentID == attachmentId);

            try
            {

                if (attachment != null)
                {
                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ImplementationImages, attachment.Target);

                    

                    await _blobStorageHelper.DeleteBlob(folder, attachmentId.ToString());
                    await _sqlHelper.DeleteImplementationAttachment(attachmentId, user.SelectedLocation);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }

            attachments = await _readOnlySqlHelper.GetImplementationAttachments(user.SelectedLocation);
            var result = TrayImplementation.GetImplementationSteps(attachments);

            return Ok(result);
        }

        [Route("trayApproval/{trayProposalId}", Name = "GetTrayApproval")]
        [HttpGet]
        public async Task<ActionResult> GetTrayApproval(int trayProposalId, int typeId)
        {
            var user = await GetUserSecurity();

            // validate location auth
            var proposedTray = (await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).First();
            if (proposedTray == null)
                return NotFound();

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ApprovalImages, trayProposalId);

            

            var binary = await _blobStorageHelper.GetBlobBytes(folder, typeId.ToString());

            return File(binary, "application/octet-stream", "TrayRationalization.pdf");
        }

        [Route("auditSummary", Name = "GetAuditSummary")]
        [HttpGet]
        public async Task<ActionResult> GetAuditSummary(string auditType, int? trayId, int? specialtyId, DateTime startDate, DateTime endDate)
        {
            var user = await GetUserSecurity();
            

            var audits = await _sqlHelper.GetProposedTrayAuditSummary(startDate, endDate, user.SelectedLocation);

            var filter = audits.Where(a => (auditType == null || a.AuditType == auditType) &&
                                           (trayId == null || a.SourceTrays.Any(t => t.TrayID == trayId)) &&
                                           (specialtyId == null || a.SpecialtyID == specialtyId));

            return Ok(filter);
        }

        [Route("surgeryTraySchedule")]
        [HttpGet]
        public async Task<ActionResult> GetSurgeryTraySchedule(int surgeryId)
        {
            var user = await GetUserSecurity();
            

            var trayProposals = await _sqlHelper.GetSurgeryTraySchedule(surgeryId, null, user.SelectedLocation);

            return Ok(new
            {
                Proposals = trayProposals
            });
        }

        [Route("traySchedule")]
        [HttpGet]
        public async Task<ActionResult> GetTraySchedule(int? surgeonId, int? trayProposalId, DateTime startDate, DateTime endDate)
        {
            var user = await GetUserSecurity();
            

            var surgeries = await _sqlHelper.SearchCaseTraySchedule(surgeonId, trayProposalId, startDate, endDate, user.SelectedLocation);
            
            return Ok(surgeries);
        }

        [Route("caseProfile")]
        [HttpGet]
        public async Task<ActionResult> GetCaseProfile(int? caseProfileId)
        {
            var user = await GetUserSecurity();
            

            CaseProfile caseProfile = new CaseProfile();
            if (caseProfileId.HasValue)
                caseProfile = await _sqlHelper.GetCaseProfile(caseProfileId.Value, user.SelectedLocation);

            return Ok(caseProfile);
        }

        [Route("caseProfiles")]
        [HttpGet]
        public async Task<ActionResult> GetCaseProfiles(int? surgeryId = null)
        {
            var user = await GetUserSecurity();
            
            List<CaseProfile> caseProfiles;
            if (surgeryId.HasValue)
                caseProfiles = new List<CaseProfile>() { await _sqlHelper.GetSurgeryCaseProfile(surgeryId.Value, user.SelectedLocation) };
            else
                caseProfiles = await _readOnlySqlHelper.GetCaseProfiles(user.SelectedLocation);

            return Ok(caseProfiles);
        }

        [Route("consolidationPlan")]
        [HttpPost]
        public async Task<ActionResult> UpdateConsolidationPlan([FromBody] ConsolidationPlanPost post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateConsolidationPlan(post.Proposals, user.SelectedLocation);
            
            return Ok(1);
        }

        [Route("surgeryCaseProfile")]
        [HttpPost]
        public async Task<ActionResult> UpdateSurgeryCaseProfile(int surgeryId, int caseProfileId, [FromBody] CaseProfileSchedulePost post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateSurgeryCaseProfile(surgeryId, caseProfileId, post.Questions, user.SelectedLocation);

            return Ok(surgeryId);
        }

        [Route("trayPhoto/{trayProposalId}")]
        [HttpGet]
        public async Task<ActionResult> GetTrayPhoto(int trayProposalId)
        {
            var user = await GetUserSecurity();
            
            

            var trayProposal = await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
            if (trayProposal.Any())
            {
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                var filename = trayProposalId.ToString();

                var exists = await _blobStorageHelper.BlobExists(folder, filename);

                if (exists)
                    return Ok(trayProposalId);
            }


            return Ok((int?)null);
        }

        // POST api/values
        [Route("rotateTrayPhoto/{trayProposalId}", Name = "RotateTrayImage")]
        [HttpPut]
        public async Task<ActionResult> RotateTrayImage(int trayProposalId, int direction)
        {
            var user = await GetUserSecurity();
            
            

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();


            var trayProposals = await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
            if (trayProposals.Any())
            {
                
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                await _blobStorageHelper.RotateImage(folder, trayProposalId.ToString(), direction);
            }

            return Ok();
        }

        // POST api/values
        [Route("trayPhoto/{trayProposalId}", Name = "DeleteTrayImage")]
        [HttpDelete]
        public async Task<ActionResult> DeleteTrayImage(int trayProposalId)
        {
            var user = await GetUserSecurity();
            
            

            var trayProposals = await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
            if (trayProposals.Any())
            {
                await _sqlHelper.UpdateProposedTrayImageFilename(trayProposalId, null, user.SelectedLocation);

                
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                await _blobStorageHelper.DeleteBlob(folder, trayProposalId.ToString());
            }
            return Ok();
        }

        [Route("trayPhoto/{trayProposalId}")]
        [HttpPut]
        public async Task<ActionResult> PutTrayPhotoBytes(IFormFile file, int trayProposalId)
        {
            var user = await GetUserSecurity();    

            try
            {
                var trayProposal = await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
                if (trayProposal.Any())
                {

                    // extract file name and file contents
                    var fileName = file.FileName;
                    byte[] fileContents;
                    using (var stream = file.OpenReadStream())
                    using (var memStream = new MemoryStream())
                    {
                        stream.CopyTo(memStream);

                        fileContents = memStream.ToArray();
                    }

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                    

                    await _sqlHelper.UpdateProposedTrayImageFilename(trayProposalId, fileName, user.SelectedLocation);
                    await _blobStorageHelper.PutBlobBytes(folder, trayProposalId.ToString(), fileContents);
                }

                return Ok(200);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        [Route("caseProfile")]
        [Route("caseProfile/{caseProfileId}")]
        [HttpPost]
        public async Task<ActionResult> PostCaseProfile([FromBody] CaseProfilePost post, int? caseProfileId = null)
        {
            var user = await GetUserSecurity();
            

            caseProfileId = await _sqlHelper.UpdateCaseProfile(caseProfileId, post.ProfileName, post.ProfileType, post.Questions,
                user.SelectedLocation);

            return Ok(caseProfileId);
        }

        [Route("scheduleRules")]
        [HttpPost]
        public async Task<ActionResult> PostTrayScheduleRules([FromBody] ScheduleRulePost post)
        {
            var user = await GetUserSecurity();
            

            var surgeonJson = JsonSerializer.Serialize(post.Surgeon);
            var categoryJson = JsonSerializer.Serialize(post.Category);

            var result = await _sqlHelper.UpdateScheduleRules(user.UserID, surgeonJson, categoryJson, post.BeginDate, post.EndDate,
                user.SelectedLocation);

            return Ok(result);
        }

        [Route("rep/{trayProposalId}")]
        [HttpPost]
        public async Task<ActionResult> PostTrayProposalRep(int trayProposalId, [FromBody] TrayProposalRepPost post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateProposedTrayRep(trayProposalId, user.UserID, post.Ignore, user.SelectedLocation);

            return Ok(trayProposalId);
        }

        [Route("log/{proposedTrayId}")]
        [HttpGet]
        public async Task<ActionResult> GetTrayProposalLog(int proposedTrayId, [FromBody] TrayProposalLogPost post)
        {
            var user = await GetUserSecurity();
            
            

            var logs = await _readOnlySqlHelper.GetProposedTrayLog(proposedTrayId, user.SelectedLocation);

            return Ok(logs);
        }

        [Route("log/{trayProposalLogId}")]
        [HttpPost]
        public async Task<ActionResult> PostTrayProposalLog(int trayProposalLogId, [FromBody] TrayProposalLogPost post)
        {
            var user = await GetUserSecurity();
            
            

            var proposedTrayId = await _sqlHelper.UpdateProposedTrayLog(trayProposalLogId, post.Requestor, post.Audience,
                post.ChangeType, post.ChangeDescription, post.AffectedItems, post.ChangeDate, post.Comments, user.SelectedLocation);

            var logs = await _readOnlySqlHelper.GetProposedTrayLog(proposedTrayId, user.SelectedLocation);

            return Ok(logs);
        }

        [Route("log/csv/{trayProposalId}/{logId}")]
        [HttpGet]
        public async Task<ActionResult> GetTrayProposalLogCsv(int trayProposalId, int logId, [FromBody] TrayProposalLogPost post)
        {
            var user = await GetUserSecurity();
            
            

            var extract = "Date, Proposed Tray, Source Tray, Instrument, Quantity\r\n";

            var proposal = await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
            var proposals = await _readOnlySqlHelper.GetProposedTrayLog(trayProposalId, user.SelectedLocation);
            var proposalLog = proposals.FirstOrDefault(p => p.TrayProposalLogID == logId);

            if (proposalLog != null)
            {
                var createDateOffset = proposalLog.ChangeDate ?? proposalLog.InsertTimestamp;
                var createDate = createDateOffset.AddHours(-5).DateTime;
                
                if (proposalLog.Instruments == null || !proposalLog.Instruments.Any())
                {
                    proposalLog.Instruments.Add(new TrayProposalInstrumentLog()
                    {
                        InstrumentName = "NO INSTRUMENTS"
                    });
                }

                foreach (var instrument in proposalLog.Instruments)
                {
                    extract += $"{createDate.ToShortDateString()}, {proposal.FirstOrDefault()?.TrayName}, {instrument.SourceTrayName}, {instrument.InstrumentName}, {instrument.Quantity}\r\n";
                }
            }


            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayRationalization.csv");
        }

        [Route("trayScheduleHistory")]
        [HttpGet]
        public async Task<ActionResult> GetProposedTrayScheduleHistory(int? caseProfileId, int? vendorId, int? surgeonId, int? trayProposalId, int? categoryId, int? questionId)
        {
            var user = await GetUserSecurity();
            

            var surgeries = await _sqlHelper.GetProposedTrayScheduleHistory(caseProfileId, vendorId, surgeonId, trayProposalId, categoryId, questionId, user.SelectedLocation);

            return Ok(new
            {
                Surgeries = surgeries
            });
        }

        [Route("traySchedule")]
        [HttpPost]
        public async Task<ActionResult> UpdateTraySchedule(int surgeryId, [FromBody] TraySchedulePost post)
        {
            var user = await GetUserSecurity();
            

            foreach (var tray in post.Trays)
            {
                if (tray.TrayProposalID == null && tray.TrayGroupID == null)
                    continue;
            
                await _sqlHelper.UpdateProposedTraySchedule(surgeryId, tray.TrayProposalID, tray.TrayGroupID, post.CaseProfiles.First()?.CaseProfileID ?? -1,
                    post.CaseProfiles.First()?.Questions, user.SelectedLocation);
            }

            return Ok(surgeryId);
        }

        [Route("surgeonPreference")]
        [HttpPost]
        public async Task<ActionResult> UpdateSurgeonPreference(int? preferenceId, [FromBody] SurgeonPreferencePost post)
        {
            var user = await GetUserSecurity();
            
            

            var trayGroup = JsonSerializer.Serialize(post.TrayGroupID);

            await _sqlHelper.UpdateSurgeonPreference(preferenceId, post.PreferenceName, post.SurgeonID, post.CaseProfileID, trayGroup,
                post.Comments, user.SelectedLocation);

            var surgeonPreferences = await _readOnlySqlHelper.GetSurgeonPreferences(user.SelectedLocation);
            var trayGroups = await _readOnlySqlHelper.GetTrayGroups(user.SelectedLocation);

            foreach (var surgeonPreference in surgeonPreferences)
            {
                var trayGroupString = trayGroups.Where(tg => surgeonPreference.TrayGroupID?.Contains(tg.TrayGroupID ?? -1) == true).Select(tg => tg.GroupName);
                surgeonPreference.TrayGroup = string.Join(",", trayGroupString);
            }

            return Ok(new {
                SurgeonPreferences = surgeonPreferences
            });
        }

        [Route("traySchedule")]
        [HttpPut]
        public async Task<ActionResult> UpdateTrayScheduleDetails(int scheduleId, [FromBody] TrayScheduleDetailPost post)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.UpdateProposedTrayScheduleDetails(scheduleId, post.Supplies, user.SelectedLocation);

            return Ok(scheduleId);
        }

        [Route("communicationRoles")]
        [HttpPost]
        public async Task<ActionResult> UpdateCommunicationRoles(int trayProposalId, [FromBody] TrayCommunicationRolePost post)
        {
            var user = await GetUserSecurity();
            
            

            await _sqlHelper.UpdateProposedTrayRoles(trayProposalId, post.OwnerID, post.Approvers, post.Users, user.SelectedLocation);

            var users = await _readOnlySqlHelper.SearchUsers(null, null, null, user.SelectedLocation);
            var proposals = await _readOnlySqlHelper.GetProposedTrays(null, user.SelectedLocation);

            return Ok(new
            {
                Users = users,
                Proposals = proposals
            });
        }

        [Route("communicationTeam")]
        [HttpGet]
        public async Task<ActionResult> GetCommunicationTeam(int trayProposalId)
        {
            var user = await GetUserSecurity();
            

            var team = await _sqlHelper.GetProposedTrayCommunicationTeam(trayProposalId, user.SelectedLocation);

            return Ok(new
            {
                Team = team,
                TrayProposalID = trayProposalId
            });
        }

        [Route("communicationHistory")]
        [HttpGet]
        public async Task<ActionResult> GetCommunication(int trayProposalId)
        {
            var user = await GetUserSecurity();
            

            var messages = await _sqlHelper.GetProposedTrayCommunication(user.UserID, trayProposalId, user.SelectedLocation);

            return Ok(new
            {
                Messages = messages
            });
        }

        [Route("communication")]
        [HttpPost]
        public async Task<ActionResult> GetCommunication([FromBody] TrayCommunicationHistoryPost post)
        {
            var user = await GetUserSecurity();
            

            var communication = await _sqlHelper.GetProposedTrayCommunicationHistory(post.PhaseID, 
                post.TrayProposalIds, post.SpecialtyIds, post.UserIds, user.SelectedLocation);

            return Ok(new
            {
                Communication = communication
            });
        }

        [Route("communicationHistory")]
        [HttpPost]
        public async Task<ActionResult> AddCommunicationHistory([FromBody] TrayCommunicationHistoryInsertPost post)
        {
            var user = await GetUserSecurity();
            

            var sentDate = DateTime.Today;
            if (DateTime.TryParse(post.SentDate, out var tmpSentDate))
                sentDate = tmpSentDate;

            var communication = await _sqlHelper.InsertProposedTrayCommunicationHistory(post.Phase, post.Activity, 
                post.Tray, sentDate, post.Audience, post.Method, user.SelectedLocation);

            return Ok();
        }

        [Route("communicationComments")]
        [HttpPost]
        public async Task<ActionResult> AddCommunicationComments(int historyId, [FromBody] TrayCommunicationHistoryUpdatePost post)
        {
            var user = await GetUserSecurity();
            

            var communication = await _sqlHelper.UpdateProposedTrayCommunicationHistory(historyId, null, post.Comments, 
                user.SelectedLocation);

            return Ok();
        }

        [Route("caseAudit")]
        [HttpPost]
        public async Task<ActionResult> AddCaseAudit(int? trayProposalId, [FromBody] AddCaseAuditPost auditPost)
        {
            var user = await GetUserSecurity();
            

            var result = -1;
            foreach (var surgeryId in auditPost.Surgeries)
            {
                if (auditPost.Target == "A")
                    result = await _sqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, null, null, user.SelectedLocation);
                else
                    result = await _sqlHelper.UpdateProposedTrayCount(trayProposalId, surgeryId, null, null, user.SelectedLocation);
            }

            return Ok(result);
        }

        [Route("caseAudit")]
        [HttpPut]
        public async Task<ActionResult> UpdateCaseAudit(int trayProposalId, int surgeryId, int? scrubTechUserId, string target)
        {
            var user = await GetUserSecurity();
            

            var result = -1;
            if (target == "A")
                result = await _sqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, scrubTechUserId, user.UserID, user.SelectedLocation);
            else
                result = await _sqlHelper.UpdateProposedTrayCount(trayProposalId, surgeryId, scrubTechUserId, user.UserID, user.SelectedLocation);

            return Ok(result);
        }

        [Route("caseAudit")]
        [HttpDelete]
        public async Task<ActionResult> DeleteCaseAudit(int trayProposalId, int surgeryId, string target)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteProposedTrayAudit(trayProposalId, surgeryId, target, user.SelectedLocation);

            return Ok(result);
        }

        [Route("comparableInstrument")]
        [HttpPost]
        public async Task<ActionResult> AddComparableInstrument(int trayProposalId, int instrumentId, int comparableInstrumentId)
        {
            var user = await GetUserSecurity();
            
            

            var instruments = await _readOnlySqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
            var trayItemId = instruments.FirstOrDefault(i => i.InstrumentID == instrumentId)?.TrayItemID;

            var result = await _sqlHelper.InsertComparableInstrument(instrumentId, trayItemId ?? -1, comparableInstrumentId, trayItemId ?? -1, user.SelectedLocation);

            return Ok(result);
        }

        [Route("dashboard/csv", Name = "GetDashboardCsv")]
        [HttpGet]
        public async Task<ActionResult> GetDashboardCsv()
        {
            var user = await GetUserSecurity();
            
            

            var proposals = await _readOnlySqlHelper.GetProposedTrays(null, user.SelectedLocation);

            var extract = "Tray, Status, Counts, Audits, Phase, Count Complete, Audit Complete, Tray Changes, Comments\r\n";
            foreach (var proposal in proposals)
            {
                extract +=  $"\"{proposal.TrayName?.Trim().Replace("\"", "\"\"")}\",{proposal.DeploymentStatusName},{proposal.Counts},{proposal.Audits},{proposal.TrayProposalPhase}," +
                    $"{proposal.CountCompleteTarget?.ToShortDateString()},{proposal.AuditCompleteTarget?.ToShortDateString()},{proposal.TrayChangesTarget?.ToShortDateString()},\"{proposal.Comments?.Trim().Replace("\"", "\"\"")}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayRationalization.csv");
        }

        [Route("implementation", Name = "GetImplementationCsv")]
        [HttpGet]
        public async Task<ActionResult> GetImplementationCsv(int target)
        {
            var user = await GetUserSecurity();
            

            var steps = TrayImplementation.GetImplementationSteps(null);
            var implementation = steps[target];

            var extract = "Service Line, Activites, Outputs, Roles, Timing, Tools\r\n";
            foreach (var specialty in implementation.Steps)
            {
                extract += $"\"{specialty.Specialty}\",\"{string.Join("\r\n", specialty.Activities)}\",\"{string.Join("\r\n", specialty.Outputs)}\",\"{string.Join("\r\n", specialty.Roles)}\"," +
                    $"\"{string.Join("\r\n", specialty.Timing)}\",\"{string.Join("\r\n", specialty.Tools)}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayImplementation.csv");
        }

        [Route("proposedTray/csv", Name = "GetTrayCsv")]
        [HttpPut]
        public async Task<ActionResult> PutTrayCsv([FromBody] TrayProposalCsvExportPost post)
        {
            if (post.Type == "export")
            {
                return await TraySummaryCsv(post.TrayProposalId);
            }

            var user = await GetUserSecurity();
            
            

            var dataTables = new List<DataTable>();
            foreach (var trayProposalId in post.TrayProposalId)
            {
                var proposedTray = await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
                var trayName = proposedTray.FirstOrDefault()?.TrayName ?? "UNKNOWN";

                var export = await _sqlHelper.GetProposedTrayInstrumentExport(trayProposalId, user.SelectedLocation);

                var proposed = export.ProposedInstruments;

                foreach (var sourceInstrument in export.SourceInstruments)
                {
                    if (export.ProposedInstruments.All(p => p.InstrumentID != sourceInstrument.InstrumentID))
                    {
                        proposed.Add(sourceInstrument);
                    }
                }

                var dtbl = post.Type == "surgical" ? GenerateSurgicalData(proposed) : GenerateSpecialistData(proposed);
                dtbl.TableName = trayName;
                dataTables.Add(dtbl);
            }

            var extractBytes = ExcelHelper.GenerateWorkbook(dataTables);

            return File(extractBytes, "application/octet-stream", "TrayImplementation.xls");
        }

        private static DataTable GenerateSpecialistData(List<TrayRationalizationExport> data)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Instrument Name");
            result.Columns.Add("Instrument Nbr");
            result.Columns.Add("Avg when used", typeof(decimal));
            result.Columns.Add("Case Usage Pcnt", typeof(decimal));
            result.Columns.Add("Original Quantity", typeof(int));
            result.Columns.Add("Proposed Quantity", typeof(int));
            result.Columns.Add("Reason for Adding");

            foreach (var instrument in data.OrderByDescending(r => r.SourceQuantity))
            {
                var drRow = result.NewRow();
                drRow["Instrument Name"] = instrument.InstrumentName?.Trim();
                drRow["Instrument Nbr"] = instrument.InstrumentNbr?.Trim();
                drRow["Avg when used"] = instrument.AvgUsed;
                drRow["Case Usage Pcnt"] = instrument.CaseUsagePcnt == 0.0M ? 0.0M : instrument.CaseUsagePcnt / 100.0M;
                drRow["Original Quantity"] = instrument.SourceQuantity;
                drRow["Proposed Quantity"] = instrument.ProposedQuantity;
                drRow["Reason for Adding"] = string.Empty;

                result.Rows.Add(drRow);
            }

            return result;
        }

        private static DataTable GenerateSurgicalData(List<TrayRationalizationExport> data)
        {
            DataTable result = new DataTable();
            result.Columns.Add("Instrument Name");
            result.Columns.Add("Quantity", typeof(int));
            result.Columns.Add("Reason for Adding");

            foreach (var instrument in data.OrderByDescending(r => r.SourceQuantity))
            {
                var drRow = result.NewRow();
                drRow["Instrument Name"] = instrument.InstrumentName?.Trim();
                drRow["Quantity"] = instrument.ProposedQuantity;
                drRow["Reason for Adding"] = string.Empty;

                result.Rows.Add(drRow);
            }

            return result;
        }

        private async Task<ActionResult> TraySummaryCsv(List<int> trayProposalIds)
        {
            var user = await GetUserSecurity();
            

            var extract = string.Empty;
            var delim = string.Empty;

            foreach (var trayProposalId in trayProposalIds)
            {
                extract += delim;

                var proposedTray = (await _readOnlySqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).First();
                var proposedInstruments = await _readOnlySqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
                var sourceTrays = await _readOnlySqlHelper.GetSourceTraySummary(trayProposalId, user.SelectedLocation);
                extract = "Tray Name, Source Tray, # Instruments, Service Line, Categories\r\n";

                foreach (var sourceTray in sourceTrays)
                {
                    extract += $"\"{proposedTray.TrayName?.Trim().Replace("\"", "\"\"")}\",{sourceTray.TrayName},{proposedInstruments.Sum(p => p.Quantity)},{proposedTray.Specialty},\"{sourceTray.CardCategories?.Trim().Replace("\"", "\"\"")}\"\r\n";
                }

                delim = "\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayRationalization.csv");
        }

        [Route("cardList/{trayProposalId}", Name = "PutCardTrayList")]
        [HttpPut]
        public async Task<ActionResult> PutCardTrayList(int trayProposalId, [FromBody] CardListPost post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.PutProposedTrayCards(trayProposalId, post.Trays, user.SelectedLocation);

            return Ok(result);
        }

        [Route("trayScheduling", Name = "GetTrayScheduling")]
        [HttpGet]
        public async Task<ActionResult> GetTrayScheduling()
        {
            var user = await GetUserSecurity();
            

            var result = await _readOnlySqlHelper.GetProposedTraySchedule(null, null, user.UserID, user.SelectedLocation);
            var rules = await _readOnlySqlHelper.GetProposedTrayScheduleRules(user.UserID, user.SelectedLocation);

            result = ApplyRules(result, rules);

            return Ok(result);
        }

        [Route("cardList/csv/{trayProposalId}", Name = "GetCardListCsv")]
        [HttpPut]
        public async Task<ActionResult> GetCardListCsv(int trayProposalId, [FromBody] CardListPost post)
        {
            var user = await GetUserSecurity();
            

            var trays = await _sqlHelper.GetProposedTrayCards(trayProposalId, post.Trays, user.SelectedLocation);

            var extract = "Surgeon, Card, Specialty, Old Tray, New Tray, Intrument, Proposed\r\n";
            foreach (var tray in trays)
            {
                extract +=
                    $"\"{tray.SurgeonName?.Trim().Replace("\"", "\"\"")}\",\"{tray.CardName?.Trim().Replace("\"", "\"\"")}\",\"{tray.SpecialtyName.Replace("\"", "\"\"")}\",\"{tray.SourceTrayName.Replace("\"", "\"\"")}\",\"{tray.NewTrayName.Replace("\"", "\"\"")}\",\"{tray.InstrumentName.Replace("\"", "\"\"")}\",\"{tray.Proposed}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "CardListExport.csv");
        }

        [Route("trayAudit/csv", Name = "GetTrayAuditCsv")]
        [HttpGet]
        public async Task<ActionResult> GetTrayAuditCsv(int? trayProposalId, string filter, DateTime? startDate, DateTime? endDate)
        {
            var user = await GetUserSecurity();
            

            var audits = new List<TraySurgeryAudit>();
            var extract = "Type, Surgery Date, OR, Status at Audit, CPT Code 1, CPT Code 2, CPT Code 3, Surgeon, Scrub Tech, Comments\r\n";
            if (filter == null || filter == "A")
            {
                audits.AddRange(await _readOnlySqlHelper.GetProposedTrayAudits(trayProposalId, startDate, endDate, user.SelectedLocation));
            }
            if (filter == null || filter == "C")
            {
                audits.AddRange(await _readOnlySqlHelper.GetProposedTrayCounts(trayProposalId, startDate, endDate, user.SelectedLocation));
            }

            foreach (var audit in audits)
            {
                extract +=
                    $"\"{audit.AuditType}\",\"{audit.ScheduleTime?.ToString("M/d/yyyy HH:mm")}\",\"{audit.RoomDescription?.Trim().Replace("\"", "\"\"")}\",\"{audit.ResearchStatus?.Replace("\"", "\"\"")}\"," +
                    $"\"{audit.CptCode1?.Replace("\"", "\"\"")}\",\"{audit.CptCode2?.Replace("\"", "\"\"")}\",\"{audit.CptCode3?.Replace("\"", "\"\"")}\",\"{audit.SurgeonName}\",\"{audit.ScrubTechUser}\",\"{audit.AuditComments?.Replace("\"", "\"\"")}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayAuditExport.csv");
        }
        [Route("trayScheduling/csv/{trayProposalId}", Name = "GetTraySchedulingCsv")]
        [HttpGet]
        public async Task<ActionResult> GetTraySchedulingCsv(int trayProposalId)
        {
            var user = await GetUserSecurity();
            

            var extract = "Tray, Instrument, Sequence, Quantity\r\n";
            var instruments = await _readOnlySqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);

            foreach (var instrument in instruments)
            {
                extract +=
                    $"\"{instrument.TrayName?.Replace("\"", "\"\"")}\",\"{instrument.InstrumentName?.Trim().Replace("\"", "\"\"")}\",{instrument.Sequence},{instrument.Quantity}\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayScheduleDetail.csv");
        }

        [Route("proposedTrayDetail/csv", Name = "GetTrayDetailCsv")]
        [HttpPut]
        public async Task<ActionResult> GetTrayDetailCsv([FromBody] TrayRationalizationDetailPost post)
        {
            var user = await GetUserSecurity();
            

            if (post.TrayIDs == null)
                post.TrayIDs = new List<TrayDetailPost>();

            var instruments = await _readOnlySqlHelper.GetProposedTrayInstruments(post.TrayProposalID, user.SelectedLocation);
            var details = new Dictionary<string, List<TrayRationalizationDetail>>();

            var comparableTrays = new List<string>();
            var sourceTrays = instruments.GroupBy(i => new { i.TrayItemID, i.TrayName })
                .Select(sourceTray => sourceTray.Key.TrayName).ToList();

            foreach (var trayId in post.TrayIDs)
            {
                var rationalization = await _readOnlySqlHelper.GetTrayRationalizationDetail(
                    post.TrayProposalID,
                    trayId.Type, trayId.ID,
                    user.SelectedLocation);

                details[$"{trayId.Type}-{trayId.ID}"] = rationalization.Instruments;

                comparableTrays.Add(rationalization.TrayName);
            }

            var extract = "Instrument";

            foreach (var tray in sourceTrays)
            {
                extract += $",{tray} - Qty,{tray} - Avg Used,{tray} - Usage %";
            }
            extract += "Proposed Qty, Proposed Avg Used,";
            foreach (var tray in comparableTrays)
            {
                extract += $",{tray ?? "No Source"} - Qty,{tray ?? "No Source"} - Avg Used";
            }
            extract += "\r\n";

            foreach (var instrument in instruments)
            {
                extract += $"\"{instrument.InstrumentName?.Trim()?.Replace("\"", "\"\"")}\"";
                foreach (var tray in sourceTrays)
                {
                    extract += instrument.TrayName == tray ?
                        $",{instrument.SourceQty},{instrument.AvgUsed},{instrument.CaseUsagePcnt}({instrument.UsedCases}/{instrument.TrayCases})" :
                        ",,,";
                }
                extract += $",{ instrument.Quantity},0";

                foreach (var trayId in post.TrayIDs)
                {
                    var rationalization = details[$"{trayId.Type}-{trayId.ID}"];
                    var mapping = rationalization.FirstOrDefault(r => r.InstrumentID == instrument.InstrumentID);

                    extract += $",{mapping?.QtyOpen},{mapping?.AvgUsed}";
                }

                extract += "\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayRationalization.csv");
        }

        [Route("trayRationalizationDetail")]
        [HttpPost]
        public async Task<ActionResult> PostTrayRationalizationDetail([FromBody] TrayRationalizationDetailPost post)
        {
            var user = await GetUserSecurity();
            
            

            if (post.TrayIDs == null)
                post.TrayIDs = new List<TrayDetailPost>();

            var result = new List<TrayRationalizationDetailItem>();

            var instruments = post.Rollup == "I" ?
                await _readOnlySqlHelper.GetProposedTrayInstruments(post.TrayProposalID, user.SelectedLocation)
                : await _sqlHelper.GetProposedTrayInstrumentCategories(post.TrayProposalID, user.SelectedLocation);


            var cardOverlaps = await _sqlHelper.GetProposedTrayCardOverlap(post.TrayProposalID, user.SelectedLocation);

            var details = new Dictionary<string, List<TrayRationalizationDetail>>();

            var comparableTrays = new List<TrayRationalizationSummary>();
            var sourceTrays = new List<TrayRationalizationSummary>();
            var sources = instruments.GroupBy(i => new {i.TrayItemID, i.TrayName});
            foreach (var source in sources)
            {
                var trayDetail = post.Rollup == "I" ?
                    await _sqlHelper.GetTrayItems(source.Key.TrayItemID, user.SelectedLocation)
                    : await _sqlHelper.GetTrayItemCategories(source.Key.TrayItemID, user.SelectedLocation);

                var sourceTray = new TrayRationalizationSummary()
                {
                    TrayItemID = (source.Key.TrayItemID == 0 ? (int?)null : source.Key.TrayItemID),
                    TrayName = source.Key.TrayName,
                    Quantity = trayDetail.Sum(s => s.Quantity)
                };

                sourceTrays.Add(sourceTray);

                foreach (var missingInstrument in trayDetail.Where(t => instruments.All(i => i.InstrumentID != t.InstrumentID)))
                {
                    var addedInstrument = new TrayRationalizationItem()
                    {
                        InstrumentID = missingInstrument.InstrumentID,
                        InstrumentName = missingInstrument.InstrumentName,
                        SourceQty = missingInstrument.Quantity
                    };
                    instruments.Add(addedInstrument);
                }
            }

            var sourceQty = sourceTrays.FirstOrDefault()?.Quantity ?? 0;

            foreach (var trayId in post.TrayIDs)
            {
                var rationalization = await _readOnlySqlHelper.GetTrayRationalizationDetail(
                    post.TrayProposalID,
                    trayId.Type, trayId.ID,
                    user.SelectedLocation);

                details[$"{trayId.Type}-{trayId.ID}"] = rationalization.Instruments;

                comparableTrays.Add(new TrayRationalizationSummary() {
                    TrayName = rationalization.TrayName,
                    Quantity = rationalization.Quantity,
                    SourceQty = sourceQty
                });
            }

            foreach (var instrument in instruments.OrderBy(i => i.InstrumentName))
            {
                var detail = new TrayRationalizationDetailItem()
                {
                    ProposedInstrument = instrument
                };

                foreach (var sourceTray in sourceTrays)
                {
                    detail.SourceInstruments.Add(
                        instrument.TrayName == sourceTray.TrayName ? instrument : null);
                }

                foreach (var trayId in post.TrayIDs)
                {
                    var rationalization = details[$"{trayId.Type}-{trayId.ID}"];

                    var compare = rationalization.FirstOrDefault(r => r.InstrumentID == instrument.InstrumentID);
                    detail.TrayInstruments.Add(compare);
                }

                result.Add(detail);
            }

            var proposedTray = new TrayRationalizationSummary()
            {
                Quantity = instruments.Sum(i => i.Quantity),
                SourceQty = sourceQty
            };

            foreach (var s in sourceTrays.Where(s => s.TrayName == null))
                s.TrayName = "No Source";

            return Ok(new
            {
                ProposedTray = proposedTray,
                Details = result,
                SourceTrays = sourceTrays,
                ComparableTrays = comparableTrays,
                Cards = cardOverlaps
            });
        }

        [Route("surgeonCards")]
        [HttpPost]
        public async Task<ActionResult> PostSurgeonCards([FromBody] TrayRationalizationConfigPost post)
        {
            var user = await GetUserSecurity();
            

            var result = new List<Card>();
            foreach (var userId in post.Surgeons)
            {
                result.AddRange(await _sqlHelper.GetCardList(userId, null, null, null, null, null, false, null, user.SelectedLocation));
            }

            return Ok(result);
        }

        [Route("trayRationalizationConfig")]
        [HttpPost]
        public async Task<ActionResult> PostTrayRationalizationConfig([FromBody] TrayRationalizationConfigPost post, int? trayProposalId = null)
        {
            var user = await GetUserSecurity();
            

            if (post.CptCode == string.Empty)
                post.CptCode = null;

            var rationalization = await _sqlHelper.GetTrayRationalization(trayProposalId,
                post.Specialties, post.Trays, post.Surgeons, post.Cards, post.CptCode, post.Questions,
                user.SelectedLocation);

            return Ok(rationalization);
        }

        [Route("trayRationalizationCompare")]
        [Route("trayRationalizationCompare/{format}")]
        [HttpPut]
        [HttpPost]
        public async Task<ActionResult> PostTrayRationalizationCompare([FromBody] TrayRationalizationComparePost post, string format = null)
        {
            var user = await GetUserSecurity();
            

            var result = new List<TrayRationalizationCompareResult>();
            foreach (var comparison in post.Comparisons)
            {
                var rationalization = await _sqlHelper.GetTrayRationalizationCompare(comparison.CustomerID, comparison.BaselineID, user.SelectedLocation);

                result.Add(rationalization);
            }

            if (format == "csv")
            {
                var extract = "Customer Tray,Instrument Category, Customer Qty, Baseline Tray, Baseline Qty";

                foreach (var comparison in result)
                {
                    foreach (var category in comparison.Instruments)
                    {
                        extract += $"\r\n{category.CustomerTrayName},{category.InstrumentCategory},{category.CustomerQuantity},{category.BaselineTrayName},{category.BaselineQuantity}";
                    }
                }

                var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
                
                return File(extractBytes, "application/octet-stream", "TrayRationalization.csv");
            }

            var summary = TrayRationalizationCompareResultSummary.SummarizeResults(result);

            return Ok(new
            {
                Summary = summary,
                ComparisonResults = result
            });
        }

        [Route("trayRationalizationOverlap/csv")]
        [HttpPost]
        public async Task<ActionResult> ExportTrayRationalizationOverlap(int trayProposalId, [FromBody] TrayRationalizationOverlapPost post)
        {
            var rationalization = await GetTrayRationalizationOverlap(trayProposalId, post.Trays);

            var extract = "Proposed\r\nInstrument,Quantity,Avg Used, Cost\r\n";
            foreach (var proposal in rationalization.Proposed)
            {
                extract += $"{proposal.TrayName}\r\n";
                foreach (var instrument in proposal.Instruments)
                {
                    extract += $"{instrument.InstrumentName}, {instrument.Quantity}, {instrument.AvgUsed}, {instrument.InstrumentCost}\r\n";
                }
            }

            extract += "\r\nShared\r\nInstrument,Quantity,Avg Used, Cost\r\n";
            foreach (var proposal in rationalization.Shared)
            {
                extract += $"{proposal.TrayName}\r\n";
                foreach (var instrument in proposal.Instruments)
                {
                    extract += $"{instrument.InstrumentName}, {instrument.Quantity}, {instrument.AvgUsed}, {instrument.InstrumentCost}\r\n";
                }
            }

            extract += "\r\nTrays\r\nInstrument,Quantity,Avg Used, Cost\r\n";
            foreach (var proposal in rationalization.Trays)
            {
                extract += $"{proposal.TrayName}\r\n";
                foreach (var instrument in proposal.Instruments)
                {
                    extract += $"{instrument.InstrumentName}, {instrument.Quantity} - {instrument.AvgUsed}, {instrument.InstrumentCost}\r\n";
                }
            }


            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            
            return File(extractBytes, "application/octet-stream", "TrayRationalizationOverlap.csv");
        }

        [Route("trayRationalizationOverlap")]
        [HttpPost]
        public async Task<ActionResult> PostTrayRationalizationOverlap(int trayProposalId, [FromBody] TrayRationalizationOverlapPost post)
        {
            var rationalization = await GetTrayRationalizationOverlap(trayProposalId, post.Trays);

            return Ok(rationalization);
        }

        private async Task<TrayRationalizationOverlap> GetTrayRationalizationOverlap(int trayProposalId, List<TrayRationalizationTrayPost> traySources)
        { 
            var user = await GetUserSecurity();
            
            

            var proposed = await _readOnlySqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
            var shared = new List<ItemTrayOverlap>();
            var trays = new List<ItemTray>();

            var traySummary = new List<TrayCardOverlapSummary>()
            {
                new TrayCardOverlapSummary()
                {
                    TrayName = "Proposed Tray",
                    NbrInstruments = proposed.Sum(p => p.Quantity),
                    CostPerTray = proposed.Sum(p => p.InstrumentCost * p.Quantity)
                }
            };

            foreach (var proposal in proposed)
            {
                if (proposal.Quantity < proposal.AvgUsed)
                    proposal.Warning = true;
            }

            foreach (var tray in traySources)
            {
                List<ItemTrayOverlap> trayInstruments;

                if (tray.TrayType == "tray")
                {
                    
                    trayInstruments = await _sqlHelper.GetTrayItemOverlaps(tray.TrayID, user.SelectedLocation);
                }
                else 
                {
                    trayInstruments = await _sqlHelper.GetProposedTrayItemOverlaps(tray.TrayID, tray.TrayLogID, user.SelectedLocation);
                }

                decimal usedInstruments = 0;
                var commonInstruments = 0;
                foreach (var instrument in trayInstruments)
                {
                    if (instrument.Quantity < instrument.AvgUsed)
                        instrument.Warning = true;

                    var matching = proposed.FirstOrDefault(p => p.InstrumentID == instrument.InstrumentID);
                    if (matching != null)
                    {
                        // If item on selected tray has qty > item on proposed tray - warn
                        instrument.Warning = instrument.Warning || (instrument.Quantity > matching.Quantity);
                        shared.Add(instrument);

                        usedInstruments += instrument.AvgUsed ?? 0; // usage history
                        commonInstruments += matching.Quantity; // proposed quantity
                    }
                    else
                    {
                        // If not added item has used qty - war
                        instrument.Warning = instrument.Warning || (instrument.AvgUsed > 0);
                        trays.Add(instrument);
                    }
                }

                var overlapSummary = new TrayCardOverlapSummary
                {
                    TrayName = trayInstruments.FirstOrDefault()?.TrayName,
                    NbrInstruments = trayInstruments.Sum(p => p.Quantity),
                    CostPerTray = trayInstruments.Sum(p => p.InstrumentCost * p.Quantity),
                    CommonInstruments = commonInstruments,
                    UsedInstruments = usedInstruments
                };

                traySummary.Add(overlapSummary);
            }


            return new TrayRationalizationOverlap()
            {
                TraySummary = traySummary,
                Proposed = proposed.GroupBy(s => "Proposed Tray").Select(s => new TrayRationalizationOverlapProposed(){ TrayName = s.Key, Instruments = s.OrderByDescending(p => p.Warning).ToList() }).ToList(),
                Shared = shared.GroupBy(s => s.TrayName).Select(s => new TrayRationalizationOverlapShared(){ TrayName = s.Key, Instruments = s.ToList() }).ToList(),
                Trays = trays.GroupBy(s => s.TrayName).Select(s => new TrayRationalizationOverlapTray (){ TrayName = s.Key, Instruments = s.ToList() }).ToList()
            };
        }

        [Route("proposedTray")]
        [HttpPost]
        public async Task<ActionResult> NewProposedTray(int? trayProposalId, [FromBody] ProposedTrayPost post)
        {
            var user = await GetUserSecurity();
            

            var trayId = await _sqlHelper.InsertProposedTray(trayProposalId, post.SpecialtyID, post.Customized ?? false, post.TrayName, post.Instruments, user.SelectedLocation);
            await _sqlHelper.InsertProposedTrayInstrumentLog(trayId, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("proposedTray")]
        [HttpPut]
        public async Task<ActionResult> UpdateProposedTray(int trayProposalId, [FromBody] ProposedTrayPost post)
        {
            var user = await GetUserSecurity();
            

            var trayGroups = new List<int>();
            foreach (var trayGroup in post.TrayGroups ?? new List<string>())
            {
                var trayGroupId = await _sqlHelper.InsertTrayGroup(trayGroup, user.SelectedLocation);
                trayGroups.Add(trayGroupId);
            }

            var trayId = await _sqlHelper.UpdateProposedTray(trayProposalId, post.TrayName, post.Status, user.UserID, post.VendorID, 
                post.SpecialtyID, post.PhaseID, post.Customized ?? false, post.CardCategories, trayGroups, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("trayGroup")]
        [HttpPost]
        public async Task<ActionResult> UpdateTrayGroup([FromBody] TrayGroupPost post)
        {
            var user = await GetUserSecurity();
            
            

            var trayGroupId = await _sqlHelper.UpdateTrayGroup(post.TrayGroupID, post.TrayGroup, post.Trays, user.SelectedLocation);

            var trayGroups = await _readOnlySqlHelper.GetTrayGroups(user.SelectedLocation);

            return Ok(new {
                TrayGroupID = trayGroupId,
                TrayGroups = trayGroups
            });
        }

        [Route("proposedTrayDashboard/{trayProposalId}")]
        [HttpPut]
        public async Task<ActionResult> UpdateProposedTrayDashboard(int trayProposalId, [FromBody] ProposedTrayDashboardPost post)
        {
            var user = await GetUserSecurity();
            

            var trayId = await _sqlHelper.UpdateProposedTrayDashboard(trayProposalId, post.Instances, post.DeploymentStatus,
                post.CountComplete, post.AuditComplete, post.TrayChanges, post.Comments, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("proposedTrayInstances")]
        [HttpPut]
        public async Task<ActionResult> UpdateProposedTrayInstances(int trayProposalId, [FromBody] ProposedTraySummaryUpdatePost post)
        {
            var user = await GetUserSecurity();
            

            var proposedInstances = post.Updates.Max(u => u.ProposedInstanceCount);

            foreach (var update in post.Updates)
            {
                var trayId = await _sqlHelper.UpdateProposedTrayInstances(trayProposalId, update.SourceTrayID,
                    proposedInstances, update.SourceInstanceCount, update.ProcessingTimes, user.SelectedLocation);
            }

            return Ok(trayProposalId);
        }

        [Route("proposedTray")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProposedTray(int trayProposalId)
        {
            var user = await GetUserSecurity();
            

            var trayId = await _sqlHelper.DeleteProposedTray(trayProposalId, user.UserID, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("proposedTrayInstruments")]
        [HttpPut]
        public async Task<ActionResult> UpdateProposedTrayInstruments(int trayProposalId, [FromBody] ProposedTrayUpdatePost post)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateProposedTrayInstruments(trayProposalId, post.Instruments, user.SelectedLocation);
            await _sqlHelper.InsertProposedTrayInstrumentLog(trayProposalId, user.SelectedLocation);

            return Ok(result);
        }

        [Route("proposedTrayQuantities")]
        [HttpPut]
        public async Task<ActionResult> UpdateProposedTrayQuantities(int trayProposalId, [FromBody] ProposedTrayUpdatePost post)
        {
            var user = await GetUserSecurity();
            

            var trayId = await _sqlHelper.UpdateProposedTrayQuantities(trayProposalId, post.Instruments, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("proposedTrayInstruments")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProposedTrayInstrument(int trayProposalId, int instrumentId)
        {
            var user = await GetUserSecurity();
            

            var trayId = await _sqlHelper.DeleteProposedTrayInstrument(trayProposalId, instrumentId, user.SelectedLocation);
            await _sqlHelper.InsertProposedTrayInstrumentLog(trayProposalId, user.SelectedLocation);

            return Ok(trayId);
        }

        [Route("auditDetails")]
        [HttpPut]
        public async Task<ActionResult> UpdateAuditDetails(int trayProposalId, string target, [FromBody] AuditDetailPost post)
        {
            var user = await GetUserSecurity();
            

            foreach (var audit in post.Audits)
            {
                await _sqlHelper.UpdateSurgeryCPTs(audit.SurgeryID, audit.SurgeryCpts, user.SelectedLocation);
                await _sqlHelper.UpdateProposedTrayAuditComments(trayProposalId, audit.SurgeryID, audit.Comments, target, user.SelectedLocation);
            }

            return Ok(trayProposalId);
        }
    }
}