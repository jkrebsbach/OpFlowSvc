using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Reporting.WebForms;
using Mindscape.Raygun4Net;
using Newtonsoft.Json;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/trayproposal")]
    public class TrayProposalController : ApiController
    {

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetTrayRationalization")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayRationalization")]
        public async Task<HttpResponseMessage> GetTrayRationalization()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var specialties = await sqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var users = await sqlHelper.SearchUsers(null, null, null, user.ProviderID, user.LocationID);
            var instrumentLookups = await sqlHelper.GetTrayInstrumentLookups(user.ProviderID, user.LocationID);
            var trays = await sqlHelper.GetItems("tray", null, null, user.ProviderID, user.LocationID);
            var collections = await sqlHelper.GetItems("collection", null, null, user.ProviderID, user.LocationID);
            var proposedTrays = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var baselineTrays = await sqlHelper.GetBaselineTrays(user.ProviderID, user.LocationID);
            var schedules = await sqlHelper.GetProposedTraySchedule(null, user.VendorID, user.UserID, user.ProviderID, user.LocationID);
            var vendors = await sqlHelper.GetVendors(user.ProviderID, user.LocationID);
            var questions = await sqlHelper.GetTrayQuestions(null, user.ProviderID, user.LocationID);
            var phases = await sqlHelper.GetTrayProposalPhases(user.ProviderID, user.LocationID);
            var cardCategories = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);
            var trayGroups = await sqlHelper.GetTrayGroups(user.ProviderID, user.LocationID);
            var instruments = await sqlHelper.GetItems("instrument", null, true, user.ProviderID, user.LocationID);
            var caseProfiles = await sqlHelper.GetCaseProfiles(user.ProviderID, user.LocationID);
            var surgeonPreferences = await sqlHelper.GetSurgeonPreferences(user.ProviderID, user.LocationID);
            var rules = await sqlHelper.GetProposedTrayScheduleRules(user.UserID, user.ProviderID, user.LocationID);
            var orgCharts = await sqlHelper.GetOrgChartAttachments(user.ProviderID, user.LocationID);

            var attachments = await sqlHelper.GetImplementationAttachments(user.ProviderID, user.LocationID);
            var implementation = TrayImplementation.GetImplementationSteps(attachments);

            schedules = ApplyRules(schedules, rules);

            if (user.VendorID.HasValue)
            {
                vendors = vendors.Where(v => v.VendorID == user.VendorID).ToList();
            }

            foreach (var surgeonPreference in surgeonPreferences)
            {
                var trayGroupString = trayGroups.Where(tg => surgeonPreference.TrayGroupID?.Contains(tg.TrayGroupID ?? -1) == true).Select(tg => tg.GroupName);
                surgeonPreference.TrayGroup = string.Join(",", trayGroupString);
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
                TrayGroups = trayGroups,
                Trays = trays,
                Schedules = schedules,
                Collections = collections,
                Proposals = proposedTrays,
                Vendors = vendors,
                BaselineTrays = baselineTrays,
                Questions = questions,
                Phases = phases,
                Instruments = instruments,
                CaseProfiles = caseProfiles,
                SurgeonPreferences = surgeonPreferences,
                Rules = rules,
                Implementation = implementation,
                OrgCharts = orgCharts
            };


            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        private static List<TrayProposalSchedule> ApplyRules(IEnumerable<TrayProposalSchedule> schedule, TrayProposalScheduleRule rules)
        {
            if (rules.Surgeon != null)
            {
                var surgeonList = JsonConvert.DeserializeObject<List<int>>(rules.Surgeon);

                if (surgeonList?.Any() == true)
                {
                    schedule = schedule.Where(s => s.SurgeryUsers.Any(su => surgeonList.Contains(su.UserID)));
                }
            }

            if (rules.Category != null)
            {
                var categoryList = JsonConvert.DeserializeObject<List<int>>(rules.Category);

                if (categoryList?.Any() == true)
                {
                    schedule = schedule.Where(s => s.CardCategories.Any(cc => categoryList.Contains(cc.CategoryID)));
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

        [SwaggerOperation("GetProposedTrays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("proposedTray")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProposedTrays()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("GetProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("proposedTray/{trayProposalId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProposedTray(int trayProposalId, int? overlapPcnt = 0)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).First();
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);
            var documents = await sqlHelper.GetProposedTrayApprovalDocuments(trayProposalId, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var trayCounts = await sqlHelper.GetTrayCountSummary(trayProposalId, user.ProviderID, user.LocationID);
            var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);
            var cardCategories = await sqlHelper.GetProposedTrayCardCategories(trayProposalId, user.ProviderID, user.LocationID);
            
            proposedTray.InstrumentCount = instruments.Sum(i => i.Quantity);
            foreach (var sourceTray in sourceTrays)
            {
                sourceTray.InstrumentCount = sourceTray.Instruments.Sum(i => i.Quantity);
                sourceTray.ProposedInstrumentCount = instruments.Sum(i => i.Quantity);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ReadOnly = (user.VendorID.HasValue && proposedTray?.VendorID != user.VendorID),
                ProposedTray = proposedTray,
                Instruments = instruments,
                ApprovalDocuments = documents,
                Audits = audits,
                Counts = counts,
                CardCategories = cardCategories,
                TrayCounts = trayCounts,
                ApprovalAudits = audits.Where(a => a.AuditUserID.HasValue).OrderBy(a => a.SurgeonName).ToList(),
                ApprovalCounts = counts.Where(c => c.AuditUserID.HasValue).OrderBy(c => c.SurgeonName).ToList(),
                SourceTrays = sourceTrays
            });
        }

        [SwaggerOperation("GetStatusLog")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationStatusLog>))]
        [Route("statusLog/{trayProposalId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetStatusLog(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var statusLog = await sqlHelper.GetProposedTrayStatusLog(trayProposalId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, statusLog);
        }

        [SwaggerOperation("GetCardOverlap")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("cardOverlap/{trayProposalId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCardOverlap(int trayProposalId, string orderBy = null, string sortBy = null, int? overlapPcnt = 0)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            
            var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);

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

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Cards = cardOverlaps.Where(i => i.OverlapPcnt >= (overlapPcnt ?? 0))
            });
        }

        [SwaggerOperation("SearchCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryAuditSearchResult>))]
        [Route("searchCases")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchCases(int trayProposalId, string target, 
            int ? surgeonUserId = null, int? specialtyId = null, int? trayId = null, int? cardId = null, 
            DateTime? beginDate = null, DateTime? endDate = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            beginDate = beginDate ?? (DateTime.Today.AddDays(-1));

            var cases = await sqlHelper.GetProposedTrayAuditSearch(trayProposalId, surgeonUserId, specialtyId, trayId, cardId, beginDate, endDate, 
                target, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cases);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("traySummary/{trayProposalId}", Name = "GetTraySummary")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTraySummary(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).FirstOrDefault();
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);
            var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);

            proposedTray.InstrumentCount = instruments.Sum(i => i.Quantity);
            foreach (var sourceTray in sourceTrays)
            {
                sourceTray.InstrumentCount = sourceTray.Instruments.Sum(i => i.Quantity);
                sourceTray.ProposedInstrumentCount = instruments.Sum(i => i.Quantity);
            }

            var imageSummary = new
            {
                ProposedTray = proposedTray,
                Instruments = instruments,
                Audits = audits.Where(a => a.AuditUserID.HasValue).OrderBy(a => a.SurgeonName).ToList(),
                Counts = counts.Where(c => c.AuditUserID.HasValue).OrderBy(c => c.SurgeonName).ToList(),
                SourceTrays = sourceTrays,
                Cards = cardOverlaps.Where(c => c.ReplaceCard).ToList()
            };
            var json = JsonConvert.SerializeObject(imageSummary);

            return ResponseHelper.PdfResponse(json);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("trayAnalyticSummary/{trayProposalId}", Name = "GetTrayAnalyticSummary")]
        [HttpPut]
        public async Task<HttpResponseMessage> GetTrayAnalyticSummary(int trayProposalId, [FromBody] TrayRationalizationReportPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).FirstOrDefault();
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);
            var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);

            proposedTray.InstrumentCount = instruments.Sum(i => i.Quantity);
            foreach (var sourceTray in sourceTrays)
            {
                sourceTray.InstrumentCount = sourceTray.Instruments.Sum(i => i.Quantity);
                sourceTray.ProposedInstrumentCount = instruments.Sum(i => i.Quantity);
            }

            // Something strange about how jquery & api controllers working here...
            if (post.SpecialtyId != null && post.SpecialtyId.Count == 1 && post.SpecialtyId[0] == 0)
                post.SpecialtyId = null;

            if (post.TrayId != null && post.TrayId.Count == 1 && post.TrayId[0] == 0)
                post.TrayId = null;

            var countAnalytics = await sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, null, null, null, null,
                user.ProviderID, user.LocationID);
            var instrumentAnalytics = await sqlHelper.GetInstrumentUsageReportData(post.SpecialtyId, null, null, null,
                null, post.TrayId, null,
                user.ProviderID, user.LocationID);
            var trayAnalytics = await sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, null, post.TrayId, null, null,
                user.ProviderID, user.LocationID);


            var parameters = new[]
            {
                new ReportParameter("Group", "t")
            };
            var countDatasets = new Dictionary<string, DataTable>
            {
                ["CountSummary"] = countAnalytics.Tables[0]
            };
            var countSummaryBytes = ReportHelper.GetReport("CountSummary", countDatasets, parameters);
            var instrumentDatasets = new Dictionary<string, DataTable>
            {
                ["InstrumentUsage"] = instrumentAnalytics.Tables[0]
            };
            var instrumentUsageBytes = ReportHelper.GetReport("InstrumentUsage", instrumentDatasets);
            var trayDatasets = new Dictionary<string, DataTable>
            {
                ["TrayRationalization"] = trayAnalytics.Tables[0]
            };
            var trayRationalizationBytes = ReportHelper.GetReport("TrayRationalization", trayDatasets);

            var reports = new[]
            {
                Convert.ToBase64String(countSummaryBytes),
                Convert.ToBase64String(instrumentUsageBytes),
                Convert.ToBase64String(trayRationalizationBytes)
            };
            

            var imageSummary = new
            {
                ProposedTray = proposedTray,
                Instruments = instruments,
                Audits = audits.Where(a => a.AuditUserID.HasValue).OrderBy(a => a.SurgeonName).ToList(),
                Counts = counts.Where(c => c.AuditUserID.HasValue).OrderBy(c => c.SurgeonName).ToList(),
                SourceTrays = sourceTrays,
                Cards = cardOverlaps.Where(c => c.ReplaceCard).ToList(),
                Reports = reports
            };
            var json = JsonConvert.SerializeObject(imageSummary);

            return ResponseHelper.PdfResponse(json);
        }

        [SwaggerOperation("PutTrayApproval")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("trayApproval/{trayProposalId}/{typeId}", Name = "PutTrayApproval")]
        public async Task<HttpResponseMessage> PutTrayApproval(int trayProposalId, int typeId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();
            int? logId = null;

            try
            {

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");
                var fileName = fileNameParam?.Value.Trim('"') ?? "";
                var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

                await sqlHelper.UpdateProposedTrayApproval(trayProposalId, fileName, typeId, user.ProviderID, user.LocationID);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ApprovalImages, trayProposalId);

                var storageHelper = BlobStorageHelper.GetHelper(user);

                await storageHelper.PutBlobBytes(folder, typeId.ToString(), fileContents);

                return Request.CreateResponse(HttpStatusCode.OK, 200);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }
        [SwaggerOperation("PutOrgChart")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("orgChart", Name = "PutOrgChart")]
        public async Task<HttpResponseMessage> PutOrgChart(string type, int? tray)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();
            
            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");
                var fileName = fileNameParam?.Value.Trim('"') ?? "";
                var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

                var orgChartId = await sqlHelper.UpdateOrgChartAttachment(type, tray, fileName, user.ProviderID, user.LocationID);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, tray ?? -1);

                var storageHelper = BlobStorageHelper.GetHelper(user);

                await storageHelper.PutBlobBytes(folder, orgChartId.ToString(), fileContents);

                var proposals = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID); 
                var orgCharts = await sqlHelper.GetOrgChartAttachments(user.ProviderID, user.LocationID);

                return Request.CreateResponse(HttpStatusCode.OK, new
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
        [SwaggerOperation("DeleteOrgChart")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpDelete]
        [Route("orgChart", Name = "DeleteOrgChart")]
        public async Task<HttpResponseMessage> DeleteOrgChart(string type, int? id)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();

            try
            {
                var storageHelper = BlobStorageHelper.GetHelper(user);

                if (type == "T")
                {
                    var proposalChecks = await sqlHelper.GetProposedTrays(id, user.ProviderID, user.LocationID);
                    var proposal = proposalChecks.First();

                    await sqlHelper.DeleteProposedTrayOrgChart(null, id, user.ProviderID, user.LocationID);

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, id ?? -1);
                    await storageHelper.DeleteBlob(folder, id.ToString());
                }
                else
                {
                    var attachments = await sqlHelper.GetOrgChartAttachments(user.ProviderID, user.LocationID);
                    var attachment = attachments.First(a => a.OrgChartID == id);

                    await sqlHelper.DeleteProposedTrayOrgChart(id, null, user.ProviderID, user.LocationID);

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, -1);
                    await storageHelper.DeleteBlob(folder, id.ToString());
                }

            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }

            var proposals = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var orgCharts = await sqlHelper.GetOrgChartAttachments(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Proposals = proposals,
                OrgCharts = orgCharts
            });
        }
        [SwaggerOperation("PutImplementation")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("implementation", Name = "PutImplementation")]
        public async Task<HttpResponseMessage> PutImplementation(int target)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");
                var fileName = fileNameParam?.Value.Trim('"') ?? "";
                var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

                var attachmentId = await sqlHelper.UpdateImplementationAttachment(target, fileName, user.ProviderID, user.LocationID);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ImplementationImages, target);

                var storageHelper = BlobStorageHelper.GetHelper(user);

                await storageHelper.PutBlobBytes(folder, attachmentId.ToString(), fileContents);

                var attachments = await sqlHelper.GetImplementationAttachments(user.ProviderID, user.LocationID);
                var result = TrayImplementation.GetImplementationSteps(attachments);

                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }
        [SwaggerOperation("GetImplementationAttachment")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpGet]
        [Route("implementationAttachment", Name = "GetImplementationAttachment")]
        public async Task<HttpResponseMessage> GetImplementationAttachment(int attachmentId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();

            try
            {
                var attachments = await sqlHelper.GetImplementationAttachments(user.ProviderID, user.LocationID);
                var attachment = attachments.First(a => a.AttachmentID == attachmentId);

                if (attachment != null)
                {
                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ImplementationImages, attachment.Target);

                    var storageHelper = BlobStorageHelper.GetHelper(user);

                    var binary = await storageHelper.GetBlobBytes(folder, attachmentId.ToString());

                    var memStream = new MemoryStream(binary);
                    var result = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(memStream)
                    };

                    result.Content.Headers.ContentDisposition =
                        new ContentDispositionHeaderValue("attachment")
                        { FileName = attachment.Filename, };

                    result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    result.Content.Headers.ContentLength = memStream.Length;

                    return result;
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }

            return Request.CreateResponse(HttpStatusCode.NotFound);
        }
        [SwaggerOperation("DeleteImplementationAttachment")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpDelete]
        [Route("implementationAttachment", Name = "DeleteImplementationAttachment")]
        public async Task<HttpResponseMessage> DeleteImplementationAttachment(int attachmentId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();
            var attachments = await sqlHelper.GetImplementationAttachments(user.ProviderID, user.LocationID);
            var attachment = attachments.FirstOrDefault(a => a.AttachmentID == attachmentId);

            try
            {

                if (attachment != null)
                {
                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ImplementationImages, attachment.Target);

                    var storageHelper = BlobStorageHelper.GetHelper(user);

                    await storageHelper.DeleteBlob(folder, attachmentId.ToString());
                    await sqlHelper.DeleteImplementationAttachment(attachmentId, user.ProviderID, user.LocationID);
                }
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }

            attachments = await sqlHelper.GetImplementationAttachments(user.ProviderID, user.LocationID);
            var result = TrayImplementation.GetImplementationSteps(attachments);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("trayApproval/{trayProposalId}", Name = "GetTrayApproval")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayApproval(int trayProposalId, int typeId)
        {
            var user = await CacheUtil.GetUserSecurity();
            
            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ApprovalImages, trayProposalId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var binary = await storageHelper.GetBlobBytes(folder, typeId.ToString());

            var memStream = new MemoryStream(binary);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayRationalization.pdf", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TraySurgeryAudit>))]
        [Route("auditSummary", Name = "GetAuditSummary")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAuditSummary(string auditType, int? trayId, int? specialtyId, DateTime startDate, DateTime endDate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var audits = await sqlHelper.GetProposedTrayAuditSummary(startDate, endDate, user.ProviderID, user.LocationID);

            var filter = audits.Where(a => (auditType == null || a.AuditType == auditType) &&
                                           (trayId == null || a.SourceTrays.Any(t => t.TrayID == trayId)) &&
                                           (specialtyId == null || a.SpecialtyID == specialtyId));

            return Request.CreateResponse(HttpStatusCode.OK, filter);
        }

        [SwaggerOperation("GetSurgeryTraySchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("surgeryTraySchedule")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetSurgeryTraySchedule(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayProposals = await sqlHelper.GetSurgeryTraySchedule(surgeryId, user.VendorID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Proposals = trayProposals
            });
        }

        [SwaggerOperation("GetTraySchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("traySchedule")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTraySchedule(int? surgeonId, int? trayProposalId, DateTime startDate, DateTime endDate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgeries = await sqlHelper.SearchCaseTraySchedule(surgeonId, trayProposalId, startDate, endDate, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, surgeries);
        }

        [SwaggerOperation("GetCaseProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseProfile")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCaseProfile(int? caseProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            CaseProfile caseProfile = new CaseProfile();
            if (caseProfileId.HasValue)
                caseProfile = await sqlHelper.GetCaseProfile(caseProfileId.Value, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, caseProfile);
        }

        [SwaggerOperation("GetCaseProfiles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseProfiles")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCaseProfiles(int? surgeryId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            List<CaseProfile> caseProfiles;
            if (surgeryId.HasValue)
                caseProfiles = new List<CaseProfile>() { await sqlHelper.GetSurgeryCaseProfile(surgeryId.Value, user.ProviderID, user.LocationID) };
            else
                caseProfiles = await sqlHelper.GetCaseProfiles(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, caseProfiles);
        }

        [SwaggerOperation("UpdateSurgeryCaseProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("surgeryCaseProfile")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateSurgeryCaseProfile(int surgeryId, int caseProfileId, [FromBody] CaseProfileSchedulePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSurgeryCaseProfile(surgeryId, caseProfileId, post.Questions, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, surgeryId);
        }

        [SwaggerOperation("GetTrayPhoto")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayPhoto/{trayProposalId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayPhoto(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayProposal = await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID);
            if (trayProposal.Any())
            {
                var blobStorage = BlobStorageHelper.GetHelper(user);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                var filename = trayProposalId.ToString();

                var exists = await blobStorage.BlobExists(folder, filename);

                if (exists)
                    return Request.CreateResponse(HttpStatusCode.OK, trayProposalId);
            }


            return Request.CreateResponse(HttpStatusCode.OK, (int?)null);
        }

        // POST api/values
        [SwaggerOperation("RotateTrayImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("rotateTrayPhoto/{trayProposalId}", Name = "RotateTrayImage")]
        [HttpPut]
        public async Task<IHttpActionResult> RotateTrayImage(int trayProposalId, int direction)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            // Make sure valid rotation direction
            if (direction != 1 && direction != -1)
                return Ok();


            var trayProposals = await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID);
            if (trayProposals.Any())
            {
                var storageHelper = BlobStorageHelper.GetHelper(user);
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                await storageHelper.RotateImage(folder, trayProposalId.ToString(), direction);
            }

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("DeleteTrayImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [Route("trayPhoto/{trayProposalId}", Name = "DeleteTrayImage")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteTrayImage(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayProposals = await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID);
            if (trayProposals.Any())
            {
                await sqlHelper.UpdateProposedTrayImageFilename(trayProposalId, null, user.ProviderID, user.LocationID);

                var storageHelper = BlobStorageHelper.GetHelper(user);
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                await storageHelper.DeleteBlob(folder, trayProposalId.ToString());
            }
            return Ok();
        }

        [SwaggerOperation("PutTrayPhotoBytes")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayPhoto/{trayProposalId}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTrayPhotoBytes(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();
            
            try
            {
                var trayProposal = await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID);
                if (trayProposal.Any())
                {
                    var provider = new MultipartMemoryStreamProvider();
                    await Request.Content.ReadAsMultipartAsync(provider);

                    // extract file name and file contents
                    var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                        .FirstOrDefault(p => p.Name.ToLower() == "filename");
                    var fileName = fileNameParam?.Value.Trim('"') ?? "";
                    var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);

                    var storageHelper = BlobStorageHelper.GetHelper(user);

                    await sqlHelper.UpdateProposedTrayImageFilename(trayProposalId, fileName, user.ProviderID, user.LocationID);
                    await storageHelper.PutBlobBytes(folder, trayProposalId.ToString(), fileContents);
                }

                return Request.CreateResponse(HttpStatusCode.OK, 200);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        [SwaggerOperation("PostCaseProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseProfile")]
        [Route("caseProfile/{caseProfileId}")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostCaseProfile([FromBody] CaseProfilePost post, int? caseProfileId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            caseProfileId = await sqlHelper.UpdateCaseProfile(caseProfileId, post.ProfileName, post.ProfileType, post.Questions,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, caseProfileId);
        }

        [SwaggerOperation("PostTrayScheduleRules")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("scheduleRules")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayScheduleRules([FromBody] ScheduleRulePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgeonJson = JsonConvert.SerializeObject(post.Surgeon);
            var categoryJson = JsonConvert.SerializeObject(post.Category);

            var result = await sqlHelper.UpdateScheduleRules(user.UserID, surgeonJson, categoryJson, post.BeginDate, post.EndDate,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("PostTrayProposalRep")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("rep/{trayProposalId}")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayProposalRep(int trayProposalId, [FromBody] TrayProposalRepPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateProposedTrayRep(trayProposalId, user.UserID, post.Ignore, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayProposalId);
        }

        [SwaggerOperation("GetTrayScheduleHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayScheduleHistory")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProposedTrayScheduleHistory(int? caseProfileId, int? vendorId, int? surgeonId, int? trayProposalId, int? categoryId, int? questionId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var surgeries = await sqlHelper.GetProposedTrayScheduleHistory(caseProfileId, vendorId, surgeonId, trayProposalId, categoryId, questionId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Surgeries = surgeries
            });
        }

        [SwaggerOperation("UpdateTraySchedule")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("traySchedule")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateTraySchedule(int surgeryId, [FromBody] TraySchedulePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            foreach (var tray in post.Trays)
            {
                if (tray.TrayProposalID == null && tray.TrayGroupID == null)
                    continue;
            
                await sqlHelper.UpdateProposedTraySchedule(surgeryId, tray.TrayProposalID, tray.TrayGroupID, post.CaseProfiles.First()?.CaseProfileID ?? -1,
                    post.CaseProfiles.First()?.Questions, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, surgeryId);
        }

        [SwaggerOperation("UpdateSurgeonPreference")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("surgeonPreference")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateSurgeonPreference(int? preferenceId, [FromBody] SurgeonPreferencePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayGroup = JsonConvert.SerializeObject(post.TrayGroupID);

            await sqlHelper.UpdateSurgeonPreference(preferenceId, post.PreferenceName, post.SurgeonID, post.CaseProfileID, trayGroup,
                post.Comments, user.ProviderID, user.LocationID);

            var surgeonPreferences = await sqlHelper.GetSurgeonPreferences(user.ProviderID, user.LocationID);
            var trayGroups = await sqlHelper.GetTrayGroups(user.ProviderID, user.LocationID);

            foreach (var surgeonPreference in surgeonPreferences)
            {
                var trayGroupString = trayGroups.Where(tg => surgeonPreference.TrayGroupID?.Contains(tg.TrayGroupID ?? -1) == true).Select(tg => tg.GroupName);
                surgeonPreference.TrayGroup = string.Join(",", trayGroupString);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new {
                SurgeonPreferences = surgeonPreferences
            });
        }

        [SwaggerOperation("UpdateTrayScheduleDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("traySchedule")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateTrayScheduleDetails(int scheduleId, [FromBody] TrayScheduleDetailPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateProposedTrayScheduleDetails(scheduleId, post.Supplies, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, scheduleId);
        }

        [SwaggerOperation("UpdateCommunicationRoles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("communicationRoles")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateCommunicationRoles(int trayProposalId, [FromBody] TrayCommunicationRolePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateProposedTrayRoles(trayProposalId, post.OwnerID, post.Approvers, post.Users, user.ProviderID, user.LocationID);

            var users = await sqlHelper.SearchUsers(null, null, null, user.ProviderID, user.LocationID);
            var proposals = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Users = users,
                Proposals = proposals
            });
        }

        [SwaggerOperation("GetCommunicationTeam")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("communicationTeam")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCommunicationTeam(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var team = await sqlHelper.GetProposedTrayCommunicationTeam(trayProposalId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Team = team,
                TrayProposalID = trayProposalId
            });
        }

        [SwaggerOperation("GetCommunication")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("communicationHistory")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetCommunication(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var messages = await sqlHelper.GetProposedTrayCommunication(user.UserID, trayProposalId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Messages = messages
            });
        }

        [SwaggerOperation("GetCommunication")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayProposalHistory>))]
        [Route("communication")]
        [HttpPost]
        public async Task<HttpResponseMessage> GetCommunication([FromBody] TrayCommunicationHistoryPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var communication = await sqlHelper.GetProposedTrayHistory(post.TrayProposalIds, post.PhaseID, post.UserID,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Communication = communication
            });
        }

        [SwaggerOperation("AddCommunicationHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("communicationHistory")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddCommunicationHistory([FromBody] TrayCommunicationHistoryInsertPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var communication = await sqlHelper.InsertProposedTrayCommunicationHistory(post.Phase, post.Activity, post.Tray, post.Audience,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [SwaggerOperation("AddCommunicationComments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("communicationComments")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddCommunicationComments(int historyId, [FromBody] TrayCommunicationHistoryUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var communication = await sqlHelper.UpdateProposedTrayCommunicationHistory(historyId, post.Comments,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        [SwaggerOperation("AddCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddCaseAudit(int trayProposalId, [FromBody] AddCaseAuditPost auditPost)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = -1;
            foreach (var surgeryId in auditPost.Surgeries)
            {
                if (auditPost.Target == "A")
                    result = await sqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, null, null, user.ProviderID, user.LocationID);
                else
                    result = await sqlHelper.UpdateProposedTrayCount(trayProposalId, surgeryId, null, null, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("UpdateCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateCaseAudit(int trayProposalId, int surgeryId, int? scrubTechUserId, string target)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = -1;
            if (target == "A")
                result = await sqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, scrubTechUserId, user.UserID, user.ProviderID, user.LocationID);
            else
                result = await sqlHelper.UpdateProposedTrayCount(trayProposalId, surgeryId, scrubTechUserId, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("DeleteCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCaseAudit(int trayProposalId, int surgeryId, string target)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProposedTrayAudit(trayProposalId, surgeryId, target, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("dashboard/csv", Name = "GetDashboardCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetDashboardCsv()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var proposals = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);

            var extract = "Tray, Status, Counts, Audits, Phase, Count Complete, Audit Complete, Tray Changes, Comments\r\n";
            foreach (var proposal in proposals)
            {
                extract +=  $"\"{proposal.TrayName?.Trim().Replace("\"", "\"\"")}\",{proposal.DeploymentStatusName},{proposal.Counts},{proposal.Audits},{proposal.TrayProposalPhase}," +
                    $"{proposal.CountCompleteTarget?.ToShortDateString()},{proposal.AuditCompleteTarget?.ToShortDateString()},{proposal.TrayChangesTarget?.ToShortDateString()},\"{proposal.Comments?.Trim().Replace("\"", "\"\"")}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                { FileName = "TrayRationalization.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("implementation", Name = "GetImplementationCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetImplementationCsv(int target)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var steps = TrayImplementation.GetImplementationSteps(null);
            var implementation = steps[target];

            var extract = "Service Line, Activites, Outputs, Roles, Timing, Tools\r\n";
            foreach (var specialty in implementation.Steps)
            {
                extract += $"\"{specialty.Specialty}\",\"{string.Join("\r\n", specialty.Activities)}\",\"{string.Join("\r\n", specialty.Outputs)}\",\"{string.Join("\r\n", specialty.Roles)}\"," +
                    $"\"{string.Join("\r\n", specialty.Timing)}\",\"{string.Join("\r\n", specialty.Tools)}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                { FileName = "TrayImplementation.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("proposedTray/csv/{trayProposalId}", Name = "GetTrayCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayCsv(int trayProposalId, string type)
        {
            if (type == "export")
            {
                return await TraySummaryCsv(trayProposalId);
            }

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var export = await sqlHelper.GetProposedTrayInstrumentExport(trayProposalId, user.ProviderID, user.LocationID);

            var proposed = export.ProposedInstruments;
            
            foreach (var sourceInstrument in export.SourceInstruments)
            {
                if (export.ProposedInstruments.All(p => p.InstrumentID != sourceInstrument.InstrumentID))
                {
                    proposed.Add(sourceInstrument);
                }
            }

            var required = new List<TrayRationalizationExport>();
            var review = new List<TrayRationalizationExport>();
            var removed = new List<TrayRationalizationExport>();

            foreach (var instrument in proposed)
            {
                if (instrument.AvgPerCase == 0)
                {
                    removed.Add(instrument);
                    continue;
                }
                var radix = instrument.AvgPerCase - (int) instrument.AvgPerCase;
                if (radix <= 0.10M)
                {
                    review.Add(instrument);
                    continue;
                }
                
                required.Add(instrument);
            }

            var categories = new string[] {"REQUIRED", "NEEDS REVIEW", "REMOVED"};
            var lists = new List<List<TrayRationalizationExport>>() {required, review, removed};

            var extract = string.Empty;
            for (var index = 0; index < 3; index++)
            {
                extract += $"{categories[index]}\r\n";
                extract += type == "surgical"
                    ? "Instrument Name, Quantity, Reason for Adding\r\n"
                    : "Instrument Name, Avg when used, Case Usage Pcnt, Original Quantity, Proposed Quantity, Reason for Adding\r\n";

                foreach (var instrument in lists[index].OrderByDescending(r => r.SourceQuantity))
                {
                    extract += type == "surgical"
                        ? $"\"{instrument.InstrumentName?.Trim().Replace("\"", "\"\"")}\",{instrument.ProposedQuantity},\r\n"
                        : $"\"{instrument.InstrumentName?.Trim().Replace("\"", "\"\"")}\",{instrument.AvgUsed:#.00},{instrument.CaseUsagePcnt:#.00}%,{instrument.SourceQuantity},{instrument.ProposedQuantity},\r\n";
                }
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayRationalization.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        private async Task<HttpResponseMessage> TraySummaryCsv(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).First();
            var proposedInstruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, false, user.ProviderID, user.LocationID);
            var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);
            var extract = "Tray Name, Source Tray, # Instruments, Service Line, Categories\r\n";

            foreach (var sourceTray in sourceTrays)
            {
                extract += $"\"{proposedTray.TrayName?.Trim().Replace("\"", "\"\"")}\",{sourceTray.TrayName},{proposedInstruments.Sum(p => p.Quantity)},{proposedTray.Specialty},\"{sourceTray.CardCategories?.Trim().Replace("\"", "\"\"")}\"\r\n";
            }
            
            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                { FileName = "TrayRationalization.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("cardList/{trayProposalId}", Name = "PutCardTrayList")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardTrayList(int trayProposalId, [FromBody] CardListPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.PutProposedTrayCards(trayProposalId, post.Trays, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayProposalSchedule>))]
        [Route("trayScheduling", Name = "GetTrayScheduling")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayScheduling()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetProposedTraySchedule(null, user.VendorID, user.UserID, user.ProviderID, user.LocationID);
            var rules = await sqlHelper.GetProposedTrayScheduleRules(user.UserID, user.ProviderID, user.LocationID);

            result = ApplyRules(result, rules);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("cardList/csv/{trayProposalId}", Name = "GetCardListCsv")]
        [HttpPut]
        public async Task<HttpResponseMessage> GetCardListCsv(int trayProposalId, [FromBody] CardListPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trays = await sqlHelper.GetProposedTrayCards(trayProposalId, post.Trays, user.ProviderID, user.LocationID);

            var extract = "Surgeon, Card, Specialty, Old Tray, New Tray, Intrument, Proposed\r\n";
            foreach (var tray in trays)
            {
                extract +=
                    $"\"{tray.SurgeonName?.Trim().Replace("\"", "\"\"")}\",\"{tray.CardName?.Trim().Replace("\"", "\"\"")}\",\"{tray.SpecialtyName.Replace("\"", "\"\"")}\",\"{tray.SourceTrayName.Replace("\"", "\"\"")}\",\"{tray.NewTrayName.Replace("\"", "\"\"")}\",\"{tray.InstrumentName.Replace("\"", "\"\"")}\",\"{tray.Proposed}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "CardListExport.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("trayAudit/csv", Name = "GetTrayAuditCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayAuditCsv(int? trayProposalId, string filter, DateTime? startDate, DateTime? endDate)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var audits = new List<TraySurgeryAudit>();
            var extract = "Type, Surgery Date, OR, Status at Audit, CPT Code 1, CPT Code 2, CPT Code 3, Surgeon, Scrub Tech, Comments\r\n";
            if (filter == null || filter == "A")
            {
                audits.AddRange(await sqlHelper.GetProposedTrayAudits(trayProposalId, startDate, endDate, user.ProviderID, user.LocationID));
            }
            if (filter == null || filter == "C")
            {
                audits.AddRange(await sqlHelper.GetProposedTrayCounts(trayProposalId, startDate, endDate, user.ProviderID, user.LocationID));
            }

            foreach (var audit in audits)
            {
                extract +=
                    $"\"{audit.AuditType}\",\"{audit.ScheduleTime?.ToString("M/d/yyyy HH:mm")}\",\"{audit.RoomDescription?.Trim().Replace("\"", "\"\"")}\",\"{audit.ResearchStatus?.Replace("\"", "\"\"")}\"," +
                    $"\"{audit.CptCode1?.Replace("\"", "\"\"")}\",\"{audit.CptCode2?.Replace("\"", "\"\"")}\",\"{audit.CptCode3?.Replace("\"", "\"\"")}\",\"{audit.SurgeonName}\",\"{audit.ScrubTechUser}\",\"{audit.AuditComments?.Replace("\"", "\"\"")}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayAuditExport.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("trayScheduling/csv/{trayProposalId}", Name = "GetTraySchedulingCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTraySchedulingCsv(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var extract = "Tray, Instrument, Sequence, Quantity\r\n";
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);

            foreach (var instrument in instruments)
            {
                extract +=
                    $"\"{instrument.TrayName?.Replace("\"", "\"\"")}\",\"{instrument.InstrumentName?.Trim().Replace("\"", "\"\"")}\",{instrument.Sequence},{instrument.Quantity}\r\n";
            }

            var extractBytes = System.Text.Encoding.Unicode.GetBytes(extract);
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                { FileName = "TrayScheduleDetail.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("proposedTrayDetail/csv", Name = "GetTrayDetailCsv")]
        [HttpPut]
        public async Task<HttpResponseMessage> GetTrayDetailCsv([FromBody] TrayRationalizationDetailPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (post.TrayIDs == null)
                post.TrayIDs = new List<TrayDetailPost>();

            var instruments = await sqlHelper.GetProposedTrayInstruments(post.TrayProposalID, false, user.ProviderID, user.LocationID);
            var details = new Dictionary<string, List<TrayRationalizationDetail>>();

            var comparableTrays = new List<string>();
            var sourceTrays = instruments.GroupBy(i => new { i.TrayItemID, i.TrayName })
                .Select(sourceTray => sourceTray.Key.TrayName).ToList();

            foreach (var trayId in post.TrayIDs)
            {
                var rationalization = await sqlHelper.GetTrayRationalizationDetail(
                    post.TrayProposalID,
                    trayId.Type, trayId.ID,
                    user.ProviderID, user.LocationID);

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
            var memStream = new MemoryStream(extractBytes);
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StreamContent(memStream)
            };

            result.Content.Headers.ContentDisposition =
                new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayRationalization.csv", };

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
            result.Content.Headers.ContentLength = memStream.Length;

            return result;
        }

        [SwaggerOperation("PostTrayRationalizationDetail")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationDetailItem>))]
        [Route("trayRationalizationDetail")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationDetail([FromBody] TrayRationalizationDetailPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (post.TrayIDs == null)
                post.TrayIDs = new List<TrayDetailPost>();

            var result = new List<TrayRationalizationDetailItem>();

            var instruments = post.Rollup == "I" ?
                await sqlHelper.GetProposedTrayInstruments(post.TrayProposalID, false, user.ProviderID, user.LocationID)
                : await sqlHelper.GetProposedTrayInstrumentCategories(post.TrayProposalID, user.ProviderID, user.LocationID);


            var details = new Dictionary<string, List<TrayRationalizationDetail>>();

            var comparableTrays = new List<TrayRationalizationSummary>();
            var sourceTrays = new List<TrayRationalizationSummary>();
            var sources = instruments.GroupBy(i => new {i.TrayItemID, i.TrayName});
            foreach (var source in sources)
            {
                var trayDetail = post.Rollup == "I" ?
                    await sqlHelper.GetTrayItems(source.Key.TrayItemID, user.ProviderID, user.LocationID)
                    : await sqlHelper.GetTrayItemCategories(source.Key.TrayItemID, user.ProviderID, user.LocationID);

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
                var rationalization = await sqlHelper.GetTrayRationalizationDetail(
                    post.TrayProposalID,
                    trayId.Type, trayId.ID,
                    user.ProviderID, user.LocationID);

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

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProposedTray = proposedTray,
                Details = result,
                SourceTrays = sourceTrays,
                ComparableTrays = comparableTrays
            });
        }

        [SwaggerOperation("PostSurgeonCards")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        [Route("surgeonCards")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSurgeonCards([FromBody] TrayRationalizationConfigPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = new List<Card>();
            foreach (var userId in post.Surgeons)
            {
                result.AddRange(await sqlHelper.GetCardList(userId, null, null, null, false, user.ProviderID, user.LocationID));
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("PostTrayRationalizationConfig")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationItem>))]
        [Route("trayRationalizationConfig")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationConfig([FromBody] TrayRationalizationConfigPost post, int? trayProposalId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (post.CptCode == string.Empty)
                post.CptCode = null;

            var rationalization = await sqlHelper.GetTrayRationalization(trayProposalId,
                post.Specialties, post.Trays, post.Surgeons, post.Cards, post.CptCode, post.Questions,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, rationalization);
        }

        [SwaggerOperation("PostTrayRationalizationCompare")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationCompareResult>))]
        [Route("trayRationalizationCompare")]
        [Route("trayRationalizationCompare/{format}")]
        [HttpPut]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationCompare([FromBody] TrayRationalizationComparePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = new List<TrayRationalizationCompareResult>();
            foreach (var comparison in post.Comparisons)
            {
                var rationalization = await sqlHelper.GetTrayRationalizationCompare(comparison.CustomerID, comparison.BaselineID, user.ProviderID, user.LocationID);

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
                var memStream = new MemoryStream(extractBytes);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(memStream)
                };

                response.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    { FileName = "TrayRationalization.csv", };

                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-steam");
                response.Content.Headers.ContentLength = memStream.Length;

                return response;
            }

            var summary = TrayRationalizationCompareResultSummary.SummarizeResults(result);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Summary = summary,
                ComparisonResults = result
            });
        }

        [SwaggerOperation("PostTrayRationalizationOverlap")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationDetail>))]
        [Route("trayRationalizationOverlap")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationOverlap(int trayProposalId, [FromBody] TrayRationalizationOverlapPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var proposed = await sqlHelper.GetProposedTrayInstruments(trayProposalId, false, user.ProviderID, user.LocationID);
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

            foreach (var tray in post.Trays)
            {
                if (string.IsNullOrEmpty(tray))
                    continue;

                var trayId = int.Parse(tray);

                var trayInstruments = await sqlHelper.GetTrayItemOverlaps(trayId, user.ProviderID, user.LocationID);

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

                        usedInstruments += instrument.AvgUsed; // usage history
                        commonInstruments += matching.Quantity; // proposed quantity
                    }
                    else
                    {
                        // If not added item has used qty - war
                        instrument.Warning = instrument.Warning || (instrument.AvgUsed > 0);
                        trays.Add(instrument);
                    }
                }

                var overlapSummary = await sqlHelper.GetTrayOverlapSummary(trayId, user.ProviderID, user.LocationID);
                overlapSummary.CommonInstruments = commonInstruments;
                overlapSummary.UsedInstruments = usedInstruments;

                traySummary.Add(overlapSummary);
            }


            if (post.StandardizedTrayID.HasValue)
            {
                var standardized = await sqlHelper.GetProposedTrayInstruments(post.StandardizedTrayID.Value, false, user.ProviderID, user.LocationID);

                decimal usedInstruments = 0;
                var commonInstruments = 0;
                foreach (var instrument in standardized)
                {
                    if (instrument.Quantity < instrument.AvgUsed)
                        instrument.Warning = true;

                    var matching = proposed.FirstOrDefault(p => p.InstrumentID == instrument.InstrumentID);
                    if (matching != null)
                    {
                        // If item on selected tray has qty > item on proposed tray - warn
                        instrument.Warning = instrument.Warning || (instrument.Quantity > matching.Quantity);
                        shared.Add(instrument);

                        usedInstruments += instrument.AvgUsed; // usage history
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
                    TrayName = "Standardized Tray",
                    NbrInstruments = standardized.Sum(p => p.Quantity),
                    CostPerTray = standardized.Sum(p => p.InstrumentCost * p.Quantity),
                    CommonInstruments = commonInstruments,
                    UsedInstruments = usedInstruments
                };

                traySummary.Add(overlapSummary);
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                TraySummary = traySummary,
                Proposed = proposed.GroupBy(s => "Proposed Tray").Select(s => new { TrayName = s.Key, Instruments = s.OrderByDescending(p => p.Warning).ToList() }),
                Shared = shared.GroupBy(s => s.TrayName).Select(s => new { TrayName = s.Key, Instruments = s.ToList()}),
                Trays = trays.GroupBy(s => s.TrayName).Select(s => new { TrayName = s.Key, Instruments = s.ToList() })
            });
        }


        [SwaggerOperation("NewProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTray")]
        [HttpPost]
        public async Task<HttpResponseMessage> NewProposedTray(int? trayProposalId, [FromBody] ProposedTrayPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.InsertProposedTray(trayProposalId, post.SpecialtyID, post.Customized, post.TrayName, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("UpdateProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTray")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTray(int trayProposalId, [FromBody] ProposedTrayPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayGroups = new List<int>();
            foreach (var trayGroup in post.TrayGroups)
            {
                var trayGroupId = await sqlHelper.InsertTrayGroup(trayGroup, user.ProviderID, user.LocationID);
                trayGroups.Add(trayGroupId);
            }

            var trayId = await sqlHelper.UpdateProposedTray(trayProposalId, post.TrayName, post.Status, user.UserID, post.VendorID, 
                post.SpecialtyID, post.PhaseID, post.Customized, post.CardCategories, trayGroups, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("UpdateTrayGroup")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("trayGroup")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateTrayGroup([FromBody] TrayGroupPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayGroupId = await sqlHelper.UpdateTrayGroup(post.TrayGroupID, post.TrayGroup, post.Trays, user.ProviderID, user.LocationID);

            var trayGroups = await sqlHelper.GetTrayGroups(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new {
                TrayGroupID = trayGroupId,
                TrayGroups = trayGroups
            });
        }

        [SwaggerOperation("UpdateProposedTrayDashboard")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayDashboard")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTrayDashboard(int trayProposalId, [FromBody] ProposedTrayDashboardPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.UpdateProposedTrayDashboard(trayProposalId, post.Instances, post.DeploymentStatus,
                post.CountComplete, post.AuditComplete, post.TrayChanges, post.Comments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("DeleteProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTray")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProposedTray(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.DeleteProposedTray(trayProposalId, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("UpdateProposedTrayInstruments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayInstruments")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTrayInstruments(int trayProposalId, [FromBody] ProposedTrayUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.UpdateProposedTrayInstruments(trayProposalId, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("UpdateProposedTrayQuantities")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayQuantities")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTrayQuantities(int trayProposalId, [FromBody] ProposedTrayUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.UpdateProposedTrayQuantities(trayProposalId, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("DeleteProposedTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayInstruments")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProposedTrayInstrument(int trayProposalId, int instrumentId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var trayId = await sqlHelper.DeleteProposedTrayInstrument(trayProposalId, instrumentId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }


        [SwaggerOperation("UpdateAuditDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("auditDetails")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateAuditDetails(int trayProposalId, string target, [FromBody] AuditDetailPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            foreach (var audit in post.Audits)
            {
                await sqlHelper.UpdateSurgeryCPTs(audit.SurgeryID, audit.SurgeryCpts, user.ProviderID, user.LocationID);
                await sqlHelper.UpdateProposedTrayAuditComments(trayProposalId, audit.SurgeryID, audit.Comments, target, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, trayProposalId);
        }
    }
}