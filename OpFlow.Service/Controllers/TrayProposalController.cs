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

            var trayId = await SqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("GetProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("proposedTray/{trayProposalId}")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProposedTray(int trayProposalId, int? overlapPcnt = 0)
        {
            var user = await CacheUtil.GetUserSecurity();

            var proposedTray = (await SqlHelper.GetProposedTrays(trayProposalId, user.ProviderID, user.LocationID)).FirstOrDefault();
            var instruments = await SqlHelper.GetProposedTrayInstruments(trayProposalId, user.ProviderID, user.LocationID);
            var cards = await SqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.ProviderID, user.LocationID);
            var audits = await SqlHelper.GetProposedTrayAudits(trayProposalId, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProposedTray = proposedTray,
                Instruments = instruments,
                Cards = cards.Where(i => i.Overlap >= (overlapPcnt ?? 0)),
                Audits = audits
            });
        }

        [SwaggerOperation("SearchCases")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ItemMaster>))]
        [Route("searchCases")]
        [HttpGet]
        public async Task<HttpResponseMessage> SearchCases(int? surgeonUserId = null, int? specialtyId = null, int? trayId = null, int? cardId = null, DateTime? serviceDate = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var beginDate = serviceDate ?? (DateTime.Today.AddDays(-1));

            var cases = await SqlHelper.SearchCases(null, surgeonUserId, null, null, null, null, specialtyId, trayId, cardId, beginDate,
                serviceDate, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cases);
        }

        [SwaggerOperation("UpdateCaseAudit")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("caseAudit")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateCaseAudit(int trayProposalId, int surgeryId, bool audited)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.UpdateProposedTrayAudit(trayProposalId, surgeryId, audited, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(string))]
        [Route("proposedTray/csv/{trayProposalId}", Name = "GetTrayCsv")]
        [HttpGet]
        public async Task<HttpResponseMessage> GetTrayCsv(int trayProposalId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var instruments = await SqlHelper.GetProposedTrayInstruments(trayProposalId, user.ProviderID, user.LocationID);

            var extract = "Instrument Name, Source Tray, Avg Used, Quantity\r\n";
            foreach (var instrument in instruments)
            {
                extract +=
                    $"\"{instrument.InstrumentName?.Trim()}\",\"{instrument.TrayName?.Trim()}\",{instrument.AvgUsed},{instrument.Quantity}\r\n";
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

            var trays = await SqlHelper.GetProposedTrayCards(trayProposalId, post.Trays, user.ProviderID, user.LocationID);

            var extract = "Surgeon, Card, Specialty, Old Tray, New Tray\r\n";
            foreach (var tray in trays)
            {
                extract +=
                    $"\"{tray.SurgeonName?.Trim()}\",\"{tray.CardName?.Trim()}\",\"{tray.SpecialtyName}\",\"{tray.SourceTrayName}\",\"{tray.NewTrayName}\"\r\n";
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(extract);
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
            
            var rationalization = await SqlHelper.GetTrayRationalizationDetail(
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

            var specialties = await SqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var surgeons = await SqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
            var proposals = await SqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var trays = await SqlHelper.GetItems("tray", null, null, user.ProviderID, user.LocationID);
            var cards = await SqlHelper.GetCards(user.ProviderID, user.LocationID);

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

            var result = new List<Card>();
            foreach (var userId in post.Surgeons)
            {
                result.AddRange(await SqlHelper.GetCardList(userId, null, null, false, user.ProviderID, user.LocationID));
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

            var rationalization = await SqlHelper.GetTrayRationalization(
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

            var rationalization = await SqlHelper.GetTrayRationalizationCompare(
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

            var rationalization = await SqlHelper.GetTrayRationalizationDetail(
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

            var proposed = await SqlHelper.GetProposedTrayInstruments(trayProposalId, user.ProviderID, user.LocationID);
            var shared = new List<ItemTray>();
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

                var trayInstruments = await SqlHelper.GetTrayItemOverlaps(trayId, user.ProviderID, user.LocationID);

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

                var overlapSummary = await SqlHelper.GetTrayOverlapSummary(trayId, user.ProviderID, user.LocationID);
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

            var trayId = await SqlHelper.InsertProposedTray(trayProposalId, post.TrayName, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }


        [SwaggerOperation("UpdateProposedTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTray")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTray(int trayProposalId, [FromBody] ProposedTrayPost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var trayId = await SqlHelper.UpdateProposedTray(trayProposalId, post.TrayName, post.Status, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("UpdateProposedTrayInstruments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayInstruments")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateProposedTrayInstruments(int trayProposalId, [FromBody] ProposedTrayUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            var trayId = await SqlHelper.UpdateProposedTrayInstruments(trayProposalId, post.Instruments, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }

        [SwaggerOperation("DeleteProposedTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [Route("proposedTrayInstruments")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProposedTrayInstrument(int trayProposalId, int instrumentId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var trayId = await SqlHelper.DeleteProposedTrayInstrument(trayProposalId, instrumentId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
        }
    }
}