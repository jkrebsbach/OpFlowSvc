using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Mindscape.Raygun4Net;
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
            var statusLog = await sqlHelper.GetProposedTrayStatusLog(trayProposalId, user.ProviderID, user.LocationID);
            var instruments = await sqlHelper.GetProposedTrayInstruments(trayProposalId, user.ProviderID, user.LocationID);
            var cards = await sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);
            var audits = await sqlHelper.GetProposedTrayAudits(trayProposalId, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                StatusLog = statusLog,
                ProposedTray = proposedTray,
                Instruments = instruments,
                Cards = cards.Where(i => i.Overlap >= (overlapPcnt ?? 0)),
                Audits = audits
            });
        }

        [SwaggerOperation("SearchCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeryAuditSearchResult>))]
        [Route("searchCases")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchCases(int? surgeonUserId = null, int? specialtyId = null, int? trayId = null, int? cardId = null, 
            DateTime? beginDate = null, DateTime? endDate = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            beginDate = beginDate ?? (DateTime.Today.AddDays(-1));

            var cases = await sqlHelper.GetProposedTrayAuditSearch(surgeonUserId, specialtyId, trayId, cardId, beginDate,
                endDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cases);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TraySurgeryAudit>))]
        [Route("auditSummary", Name = "GetAuditSummary")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetAuditSummary()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var audits = await sqlHelper.GetProposedTrayAuditSummary(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, audits);
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
                result = await sqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, null, null, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("UpdateCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateCaseAudit(int trayProposalId, int surgeryId, int? scrubTechUserId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, scrubTechUserId, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("DeleteCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCaseAudit(int trayProposalId, int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var result = await sqlHelper.DeleteProposedTrayAudit(trayProposalId, surgeryId, user.ProviderID, user.LocationID);

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
                    : "Instrument Name, Original Quantity, Avg when used, Avg per case, Proposed Quantity, Reason for Adding\r\n";

                foreach (var instrument in lists[index].OrderByDescending(r => r.SourceQuantity))
                {
                    extract += type == "surgical"
                        ? $"\"{instrument.InstrumentName?.Trim().Replace("\"", "\"\"")}\",{instrument.ProposedQuantity},\r\n"
                        : $"\"{instrument.InstrumentName?.Trim().Replace("\"", "\"\"")}\",{instrument.SourceQuantity},{instrument.AvgPerCase:#.00},{instrument.ProposedQuantity},\r\n";
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
            var trays = await sqlHelper.GetItems("tray", null, null, user.ProviderID, user.LocationID);
            var cards = await sqlHelper.GetCards(user.ProviderID, user.LocationID);

            var result = new TrayRationalizationHeader()
            {
                Specialties = specialties,
                Surgeons = surgeons,
                Trays = trays,
                Proposals = proposals,
                Cards = cards
            };


            return Request.CreateResponse(HttpStatusCode.OK, result);
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayRationalization>))]
        [Route("trayRationalizationConfig")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostTrayRationalizationConfig([FromBody] TrayRationalizationConfigPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var rationalization = await sqlHelper.GetTrayRationalization(
                post.Specialties, post.Trays, post.Surgeons, post.Cards,
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

            var proposed = await sqlHelper.GetProposedTrayInstruments(trayProposalId, user.ProviderID, user.LocationID);
            var shared = new List<ItemTray>();
            var trays = new List<ItemTray>();

            proposed = proposed.Where(p => p.HistoryType == null).ToList();

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

                var usedInstruments = 0;
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

            var trayId = await sqlHelper.UpdateProposedTray(trayProposalId, post.TrayName, post.Status, user.UserID, user.ProviderID, user.LocationID);

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
    }
}