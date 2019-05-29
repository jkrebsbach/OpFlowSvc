using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Mindscape.Raygun4Net;
using Newtonsoft.Json;
using OpFlow.Data;
using OpFlow.Data.Administration;
using OpFlow.Data.Analytics;
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(TrayRationalizationHeader))]
        [Route("trayRationalization")]
        public async Task<HttpResponseMessage> GetTrayRationalization()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var specialties = await sqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var surgeons = await sqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
            var proposals = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var instrumentCategories = await sqlHelper.GetProposedTrayInstrumentCategories(user.ProviderID, user.LocationID);
            var trays = await sqlHelper.GetItems("tray", null, null, user.ProviderID, user.LocationID);
            var cards = await sqlHelper.GetCards(user.ProviderID, user.LocationID);
            var proposedTrays = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var vendors = await sqlHelper.GetVendors(user.ProviderID, user.LocationID);
            var questions = await sqlHelper.GetTrayQuestions(null, user.ProviderID, user.LocationID);
            var phases = await sqlHelper.GetTrayProposalPhases(user.ProviderID, user.LocationID);

            var result = new TrayRationalizationHeader()
            {
                Specialties = specialties,
                Surgeons = surgeons,
                Categories = instrumentCategories,
                Trays = trays,
                Proposals = proposals,
                Cards = cards,
                Vendors = vendors,
                StandardizedTrays = proposedTrays.Where(p => p.Status == "D").ToList(),
                Vendor = user.RoleType == "External",
                Questions = questions,
                Phases = phases
            };


            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("GetProposedTrays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("proposedTray")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProposedTrays()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).FirstOrDefault();
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);
            var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ReadOnly = (user.VendorID.HasValue && proposedTray?.VendorID != user.VendorID),
                ProposedTray = proposedTray,
                Instruments = instruments,
                Cards = cardOverlaps.Where(i => i.Overlap >= (overlapPcnt ?? 0)),
                Audits = audits,
                Counts = counts,
                ApprovalAudits = audits.OrderBy(a => a.SurgeonName).ToList(),
                ApprovalCounts = counts.OrderBy(c => c.SurgeonName).ToList(),
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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var statusLog = await sqlHelper.GetProposedTrayStatusLog(trayProposalId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, statusLog);
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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var proposedTray = (await sqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).FirstOrDefault();
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, true, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var counts = await sqlHelper.GetProposedTrayCounts(trayProposalId, null, null, user.ProviderID, user.LocationID);
            var sourceTrays = await sqlHelper.GetSourceTraySummary(trayProposalId, user.ProviderID, user.LocationID);
            var cardOverlaps = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);

            //var imageBytes = ImageHelper.GenerateSummaryPDF(proposedTray, instruments, audits, counts, sourceTrays);
            var imageSummary = new
            {
                ProposedTray = proposedTray,
                Instruments = instruments,
                Audits = audits.OrderBy(a => a.SurgeonName).ToList(),
                Counts = counts.OrderBy(c => c.SurgeonName).ToList(),
                SourceTrays = sourceTrays,
                Cards = cardOverlaps.Where(c => c.ReplaceCard).ToList()
            };
            var json = JsonConvert.SerializeObject(imageSummary);

            var opflowPdf = ConfigurationManager.AppSettings["OpFlowPDF"];
            var request = (HttpWebRequest) WebRequest.Create(opflowPdf);
            request.ContentType = "application/json";
            request.Method = HttpMethod.Post.Method;

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            byte[] buffer = new byte[1024];
            long received = 0;
            var memStream = new MemoryStream();
            var httpResponse = (HttpWebResponse) request.GetResponse();
            using (var input = httpResponse.GetResponseStream())
            {
                long size = input.Read(buffer, 0, buffer.Length);
                while (size > 0)
                {
                    memStream.Write(buffer, 0, (int)size);
                    received += size;

                    size = input.Read(buffer, 0, buffer.Length);
                }
            }

            memStream.Position = 0;
            //byte[] imageBytes = null;
            //var memStream = new MemoryStream(imageBytes);
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

        [SwaggerOperation("PutTrayApproval")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("trayApproval/{trayProposalId}/{type}", Name = "PutTrayApproval")]
        public async Task<HttpResponseMessage> PutTrayApproval(int trayProposalId, string type)
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
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

                await sqlHelper.UpdateProposedTrayApproval(trayProposalId, fileName, type, user.ProviderID, user.LocationID);

                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ApprovalImages, trayProposalId);

                var storageHelper = BlobStorageHelper.GetHelper(user);

                var filename = (type == "A" ? "Approval" : "Rollout");
                await storageHelper.PutBlobBytes(folder, filename, fileContents);

                return Request.CreateResponse(HttpStatusCode.OK, 200);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("trayApproval/{trayProposalId}", Name = "GetTrayApproval")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayApproval(int trayProposalId, string type)
        {
            var user = await CacheUtil.GetUserSecurity();
            
            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.ApprovalImages, trayProposalId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var filename = (type == "A" ? "Approval" : "Rollout");
            var binary = await storageHelper.GetBlobBytes(folder, filename);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var audits = await sqlHelper.GetProposedTrayAuditSummary(startDate, endDate, user.ProviderID, user.LocationID);

            var filter = audits.Where(a => (auditType == null || a.AuditType == auditType) &&
                                           (trayId == null || a.SourceTrays.Any(t => t.TrayID == trayId)) &&
                                           (specialtyId == null || a.SpecialtyID == specialtyId));

            return Request.CreateResponse(HttpStatusCode.OK, filter);
        }

        [SwaggerOperation("AddCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddCaseAudit(int trayProposalId, [FromBody] AddCaseAuditPost auditPost)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.DeleteProposedTrayAudit(trayProposalId, surgeryId, target, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("proposedTray/csv/{trayProposalId}", Name = "GetTrayCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayCsv(int trayProposalId, string type)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("cardList/{trayProposalId}", Name = "PutCardTrayList")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardTrayList(int trayProposalId, [FromBody] CardListPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.PutProposedTrayCards(trayProposalId, post.Trays, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("cardList/csv/{trayProposalId}", Name = "GetCardListCsv")]
        [HttpPut]
        public async Task<HttpResponseMessage> GetCardListCsv(int trayProposalId, [FromBody] CardListPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
        [Route("proposedTrayDetail/csv", Name = "GetTrayDetailCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayDetailCsv(int? cardId, int? trayId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var rationalization = await sqlHelper.GetTrayRationalizationDetail(
                cardId, trayId,
                user.ProviderID, user.LocationID);

            var extract = "Surgeon,Card,Tray,Instrument,Qty Open,Avg Used,Peel Pack Qty,Peel Pack Status\r\n";
            foreach (var detail in rationalization)
            {
                extract +=
                    $"\"{detail.SurgeonName?.Trim()}\",\"{detail.CardName?.Trim()}\",\"{detail.TrayName?.Trim()}\",{detail.InstrumentName?.Trim()},{detail.QtyOpen},{detail.AvgUsed},{detail.PeelPackQty},{detail.PeelPackStatus}\r\n";
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

        [SwaggerOperation("PostSurgeonCards")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        [Route("surgeonCards")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSurgeonCards([FromBody] TrayRationalizationConfigPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = new List<Card>();
            foreach (var userId in post.Surgeons)
            {
                result.AddRange(await sqlHelper.GetCardList(userId, null, null, false, user.ProviderID, user.LocationID));
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("PostTrayRationalizationConfig")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationItem>))]
        [Route("trayRationalizationConfig")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationConfig(int trayProposalId, [FromBody] TrayRationalizationConfigPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            if (post.CptCode == string.Empty)
                post.CptCode = null;

            var rationalization = await sqlHelper.GetTrayRationalization(trayProposalId,
                post.Specialties, post.Trays, post.Surgeons, post.Cards, post.CptCode, post.Questions,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, rationalization);
        }

        [SwaggerOperation("PostTrayRationalizationCompare")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationCompare>))]
        [Route("trayRationalizationCompare")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationCompare([FromBody] TrayRationalizationComparePost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var rationalization = await sqlHelper.GetTrayRationalizationCompare(
                post.TrayID, post.Overlap, post.Buffer,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, rationalization.Where(r => r.OverlapPcnt > post.Overlap));
        }

        [SwaggerOperation("PostTrayRationalizationDetail")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationDetail>))]
        [Route("trayRationalizationDetail")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationDetail([FromBody] TrayRationalizationDetailPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var rationalization = await sqlHelper.GetTrayRationalizationDetail(
                post.CardID, post.TrayID,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, rationalization);
        }

        [SwaggerOperation("PostTrayRationalizationOverlap")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalizationDetail>))]
        [Route("trayRationalizationOverlap")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationOverlap(int trayProposalId, [FromBody] TrayRationalizationOverlapPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var trayId = await sqlHelper.InsertProposedTray(trayProposalId, post.TrayName, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("UpdateProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTray")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTray(int trayProposalId, [FromBody] ProposedTrayPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var trayId = await sqlHelper.UpdateProposedTray(trayProposalId, post.TrayName, post.Status, user.UserID, post.VendorID, 
                post.SpecialtyID, post.PhaseID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("DeleteProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTray")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProposedTray(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var trayId = await sqlHelper.UpdateProposedTrayInstruments(trayProposalId, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("DeleteProposedTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayInstruments")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProposedTrayInstrument(int trayProposalId, int instrumentId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

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
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            foreach (var audit in post.Audits)
            {
                await sqlHelper.UpdateSurgeryCPTs(audit.SurgeryID, audit.SurgeryCpts, user.ProviderID, user.LocationID);
                await sqlHelper.UpdateProposedTrayAuditComments(trayProposalId, audit.SurgeryID, audit.Comments, target, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, trayProposalId);
        }
    }
}