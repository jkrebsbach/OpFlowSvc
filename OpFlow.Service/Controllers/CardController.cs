using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Mindscape.Raygun4Net;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/card")]
    public class CardController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> Get(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cards = await sqlHelper.GetCardData(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cards);
        }

        [SwaggerOperation("GetById")]
        [Route("details")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardDetail))]
        public async Task<HttpResponseMessage> GetDetails(int? cardId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            CardDetail card = null;
            if (cardId.HasValue)
            {
                var cards = await sqlHelper.GetCardData(cardId.Value, user.ProviderID, user.LocationID);

                card = cards.FirstOrDefault();
                if (card == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound);
                }

                card.CardUsers = await sqlHelper.GetCardUsers(cardId.Value, user.ProviderID, user.LocationID, 1);
                card.CardItems = await sqlHelper.GetCardItems(cardId.Value, user.ProviderID, user.LocationID);
                card.SurgeryAdditionalItems = await sqlHelper.GetCardAdditionalItems(cardId.Value, user.ProviderID, user.LocationID);
                card.CardProcedures = await sqlHelper.GetCardProcedures(cardId.Value, user.ProviderID, user.LocationID);
                card.CardCategories = await sqlHelper.GetCardCategoryXRef(cardId.Value, user.ProviderID, user.LocationID);
            }

            var cardCategories = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);
            var surgeons = await sqlHelper.SearchUsers(null, 1, null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                CardCategories = cardCategories,
                Card = card,
                Surgeons = surgeons
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetUsageHistory")]
        [Route("usageHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardUsageHistory))]
        public async Task<HttpResponseMessage> GetUsageHistory(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardUsageHistory(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardItems")]
        [Route("carditems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItem>))]
        public async Task<HttpResponseMessage> GetCardItems(int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardSurgeryDelays")]
        [Route("delays")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardSurgeryDelays(int surgeryId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardSurgeryDelays(surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardList")]
        [Route("list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        public async Task<HttpResponseMessage> GetCardList(int? userId = null, int? specialtyId = null, int? procedureId = null, int? bundleId = null, bool? defaultFilter = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var defaultCardOnly = defaultFilter ?? false;
            var cardList = await sqlHelper.GetCardList(userId, specialtyId, procedureId, bundleId, defaultCardOnly,
                user.ProviderID, user.LocationID);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("UsedCardList")]
        [Route("listUsed")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Card>))]
        [HttpPost]
        public async Task<HttpResponseMessage> UsedCardList([FromBody] UsedCardSearchPost post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cardList = await sqlHelper.GetUsedCardList(post.UserIDs, post.TrayIDs, post.CategoryIDs,
                user.ProviderID, user.LocationID);
            var result = cardList.OrderBy(c => c.CardDescription).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetEditFeedback")]
        [Route("feedback")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardItemFeedback>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetEditFeedback(int? specialtyId = null, int? userId = null, int? cardId = null, DateTime? beginDate = null, DateTime? endDate = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardFeedback(specialtyId, userId, cardId, beginDate, endDate,
                user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCategories")]
        [Route("cardCategories")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        public async Task<HttpResponseMessage> GetCardCategories()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileCpt")]
        [Route("procedureProfileCpt")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileCpt(int procedureProfileId, string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileCpt(procedureProfileId, cptCode, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileTray")]
        [Route("procedureProfileTray")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileTray(int procedureProfileId, int trayItemId, string trayType)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, trayType, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileItem")]
        [Route("procedureProfileItem")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileItem(int procedureProfileId, string itemType, int itemId, string category, int quantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (itemType == "I") // Instrument
                await sqlHelper.UpdateProcedureProfileItem(procedureProfileId, null, itemId, category, quantity, user.ProviderID, user.LocationID);
            else // Supply
                await sqlHelper.UpdateProcedureProfileItem(procedureProfileId, itemId, null, category, quantity, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, itemId);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileCard")]
        [Route("procedureProfileCard")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileCard(int procedureProfileId, int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateProcedureProfileCard(procedureProfileId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cardId);
        }

        // GET api/values/5
        [SwaggerOperation("PostProcedureProfileTrayInstrument")]
        [Route("procedureProfileTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostProcedureProfileTrayInstrument(int procedureProfileId, int itemId, int trayItemId, int? categoryId, int quantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateProcedureProfileTrayInstrument(procedureProfileId, itemId, trayItemId, categoryId, quantity, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, itemId);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteProcedureProfileCpt")]
        [Route("procedureProfileCpt")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureProfileCpt(int procedureProfileId, string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileCpt(procedureProfileId, cptCode, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteProcedureProfileItem")]
        [Route("procedureProfileItem")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureProfileItem(int procedureProfileId, string itemType, int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (itemType == "I") // Instrument
                await sqlHelper.DeleteProcedureProfileItem(procedureProfileId, null, itemId, user.ProviderID, user.LocationID);
            else // Supply
                await sqlHelper.DeleteProcedureProfileItem(procedureProfileId, itemId, null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, itemId);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteProcedureProfileTrayInstrument")]
        [Route("procedureProfileTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureProfileTrayInstrument(int procedureProfileId, string trayType, int itemId, int trayItemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteProcedureProfileTrayInstrument(procedureProfileId, trayType, itemId, trayItemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, itemId);
        }

        // GET api/values/5
        [SwaggerOperation("GetCptCodes")]
        [Route("cptCode")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        public async Task<HttpResponseMessage> GetCptCodes()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCptCodes(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfiles")]
        [Route("procedureProfiles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfile>))]
        public async Task<HttpResponseMessage> GetProcedureProfiles()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetProcedureProfile(null, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfile")]
        [Route("procedureProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        public async Task<HttpResponseMessage> GetProcedureProfile(int? procedureProfileId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            ProcedureProfile profile = null;
            var trayUsage = new List<ProcedureProfileTrayUsage>();
            if (procedureProfileId.HasValue)
            {
                var profiles = await sqlHelper.GetProcedureProfile(procedureProfileId, user.ProviderID, user.LocationID);
                profile = profiles.FirstOrDefault();


                trayUsage = await sqlHelper.GetProcedureProfileTrayUsage(procedureProfileId.Value, user.ProviderID, user.LocationID);
            }
            var cardCategories = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);
            var specialties = await sqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var cards = await sqlHelper.GetCards(user.ProviderID, user.LocationID);
            var trays = await sqlHelper.GetItems("TRAY", null, null, user.ProviderID, user.LocationID);
            var proposed = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var itemCategories = await sqlHelper.GetItemCategories(user.ProviderID, user.LocationID);
            var instrumentCategories = await sqlHelper.GetInstrumentCategories(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProcedureProfile = profile,
                CardCategories = cardCategories,
                Specialties = specialties,
                Cards = cards,
                Trays = trays,
                Proposed = proposed,
                TrayUsage = trayUsage,
                ItemCategories = itemCategories,
                InstrumentCategories = instrumentCategories
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileChart")]
        [Route("procedureProfileChart")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        public async Task<HttpResponseMessage> GetProcedureProfileChart(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var analytics = await sqlHelper.GetProcedureProfileReport(procedureProfileId, user.ProviderID, user.LocationID);

            var profile = analytics.Tables[0].DefaultView;
            var datasets = new Dictionary<string, DataTable>
            {
                ["ProcedureProfile"] = profile.ToTable()
            };

            var reportName = "ProcedureProfile";

            var result = ReportHelper.GetReport(reportName, datasets);
            var webImage = ImageHelper.CreateWebImage(result);

            return ResponseHelper.ImageResponse(Request, webImage);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileTrayAlignment")]
        [Route("procedureProfileTrayAlignment")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        public async Task<HttpResponseMessage> GetProcedureProfileTrayAlignment(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var analytics = await sqlHelper.GetProcedureProfileTrayAlignment(procedureProfileId, user.ProviderID, user.LocationID);

            var profile = analytics.Tables[0].DefaultView;
            var datasets = new Dictionary<string, DataTable>
            {
                ["Alignment"] = profile.ToTable()
            };

            var reportName = "ProfileAlignment";

            var result = ReportHelper.GetReport(reportName, datasets);
            var webImage = ImageHelper.CreateWebImage(result);

            var summary = analytics.Tables[1].DataTableToList<ProcedureProfileAnalyticsSummary>();

            return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileSupplyAlignment")]
        [Route("procedureProfileSupplyAlignment")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        public async Task<HttpResponseMessage> GetProcedureProfileSupplyAlignment(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var analytics = await sqlHelper.GetProcedureProfileSupplyAlignment(procedureProfileId, user.ProviderID, user.LocationID);

            var profile = analytics.Tables[0].DefaultView;
            var datasets = new Dictionary<string, DataTable>
            {
                ["Alignment"] = profile.ToTable()
            };

            var reportName = "ProfileAlignment";

            var result = ReportHelper.GetReport(reportName, datasets);
            var webImage = ImageHelper.CreateWebImage(result);

            var summary = analytics.Tables[1].DataTableToList<ProcedureProfileAnalyticsSummary>();

            return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileCompare")]
        [Route("procedureProfileCompare")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfileCardComparison>))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfileCompare(int procedureProfileId, [FromBody]ProcedureProfileCardComparisonRequest request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var profile = (await sqlHelper.GetProcedureProfile(procedureProfileId, user.ProviderID, user.LocationID)).First();

            var traySummary = profile.TrayItems.GroupBy(p => p.TrayName).Select(p => new ProfileItem()
            { 
                ItemType = "TRAY",
                ItemDescription = p.Key,
                Quantity = p.Sum(ti => ti.Quantity)
            });

            
            var profileItems = profile.Items.Union(traySummary);
            List<CardItem> studyItems = new List<CardItem>();


            foreach (var cardId in request.Trays) 
            {
                var cardItems = await sqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);
                studyItems.AddRange(cardItems);
            }

            foreach (var trayId in request.Trays)
            {
                var trayItems = await sqlHelper.GetTrayItems(trayId, user.ProviderID, user.LocationID);
                if (trayItems.Any())
                {
                    studyItems.Add(new CardItem()
                    {
                        Category = "TRAY",
                        ItemDescription = trayItems.First().TrayName,
                        Quantity = trayItems.Sum(ti => ti.Quantity)
                    });
                }
            }

            var result = new List<ProcedureProfileCardComparison>();
            result.Add(new ProcedureProfileCardComparison()
            {
                ItemType = "INSTRUMENT",
                CardItems = studyItems.Where(ci => ci.Category != "TRAY" && ci.ItemType == "INSTRUMENT").ToList(),
                ProfileItems = profileItems.Where(p => p.ItemType == "I").ToList()
            });
            result.Add(new ProcedureProfileCardComparison()
            {
                ItemType = "SUPPLY",
                CardItems = studyItems.Where(ci => ci.ItemType == "SUPPLY").ToList(),
                ProfileItems = profileItems.Where(p => p.ItemType == "S").ToList()
            });
            result.Add(new ProcedureProfileCardComparison()
            {
                ItemType = "TRAY",
                CardItems = studyItems.Where(ci => ci.Category == "TRAY").ToList(),
                ProfileItems = profileItems.Where(p => p.ItemType == "TRAY").ToList()
            });

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("GetProcedureProfileVenn")]
        [Route("procedureProfileVenn")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfileCardComparison>))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfileVenn(int procedureProfileId, [FromBody]ProcedureProfileCardComparisonRequest request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cardItems = new List<CardItem>();
            var profile = (await sqlHelper.GetProcedureProfile(procedureProfileId, user.ProviderID, user.LocationID)).First();
            foreach (var cardId in request.Cards)
            {
                var items = await sqlHelper.GetCardItems(cardId, user.ProviderID, user.LocationID);
                cardItems.AddRange(items);
            }

            foreach (var trayId in request.Trays)
            {
                var items = await sqlHelper.GetTrayItems(trayId, user.ProviderID, user.LocationID);
                foreach (var item in items)
                {
                    cardItems.Add(new CardItem()
                    {
                        TrayName = item.TrayName,
                        TrayID = item.TrayItemID,
                        Quantity = item.Quantity,
                        UnitCost = item.InstrumentCost,
                        ItemDescription = item.ItemDescription,
                        AvgUsed = item.AvgUsed
                    });
                }
            }

            var profileTrayItems = profile.TrayItems;
            var shared = profileTrayItems.Where(t => cardItems.Any(ci => ci.TrayID == t.TrayID)).ToList();

            foreach(var sharedItem in shared)
            {
                profileTrayItems.RemoveAll(p => p.TrayID == sharedItem.TrayID && p.ItemID == sharedItem.ItemID);
            }
            
            var profileTrays = profileTrayItems.GroupBy(p => p.TrayName).Select(t =>
                new TrayCardOverlapSummary()
                {
                    TrayName = t.Key,
                    NbrInstances = t.First().NbrInstances,
                    NbrInstruments = t.Sum(i => i.Quantity),
                    CostPerTray = t.Sum(p => p.UnitCost * p.Quantity)
                });


            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                TraySummary = profileTrays,
                Proposed = profileTrayItems.GroupBy(p => p.TrayName).Select(p =>
                new
                {
                    TrayName = p.Key,
                    Instruments = p.ToList()
                }),
                Shared = shared.GroupBy(p => p.TrayName).Select(p =>
                new
                {
                    TrayName = p.Key,
                    Instruments = p.ToList()
                }),
                Card = cardItems.GroupBy(c => c.TrayName).Select(c =>
                new
                {
                    TrayName = c.Key ?? "ITEMS",
                    Instruments = c.ToList()
                })
            });
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfile")]
        [Route("procedureProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> PutProcedureProfile(int? procedureProfileId, [FromBody] ProcedureProfilePost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfile(procedureProfileId, request.ProfileName, request.CardCategoryID, request.SpecialtyID,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileDashboard")]
        [Route("procedureProfileDashboard")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> PutProcedureProfileDashboard(int? procedureProfileId, [FromBody] ProcedureProfileDashboardPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileDashboard(procedureProfileId, request.Cards, request.Trays, request.Proposals,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostEditFeedback")]
        [Route("feedback")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostEditFeedback(int feedbackId, [FromBody]FeedbackRequest post)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateCardFeedback(feedbackId, post.Response, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("users")]
        [SwaggerOperation("GetCardUsers")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardUser>))]
        public async Task<HttpResponseMessage> GetCardUsers(int cardId, int? typeId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetCardUsers(cardId, user.ProviderID, user.LocationID, typeId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("bundledefault")]
        [SwaggerOperation("GetBundleDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public async Task<HttpResponseMessage> GetBundleDefaultCardFlowRoom(int bundleId, int? userId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetBundleDefaultCardFlowRoom(bundleId, userId ?? user.UserID, user.ProviderID, user.LocationID) ??
                new CardFlowRoom()
                {
                    CardDescription = "None",
                    FlowDescription = "None",
                    RoomDescription = "None"
                };

            result.Cards = await sqlHelper.GetCardList(userId, null, null, bundleId, false, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("importSurgeons")]
        [SwaggerOperation("GetImportSurgeons")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Surgeon>))]
        public async Task<HttpResponseMessage> GetImportSurgeons()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetImportSurgeons(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("importProcedures")]
        [SwaggerOperation("GetImportProcedures")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        public async Task<HttpResponseMessage> GetImportProcedures(string importSurgeon)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetImportProcedures(importSurgeon, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("proceduredefault")]
        [SwaggerOperation("GetProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var cards = await sqlHelper.GetProcedureDefaultCardFlowRoom(
                user.ProviderID, user.LocationID, cptCode);
            var result = cards.FirstOrDefault();

            return result == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("specialtyproceduredefault")]
        [SwaggerOperation("GetSpecialtyProcedureDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(CardFlowRoom))]
        public async Task<HttpResponseMessage> GetSpecialtyProcedureDefaultCardFlowRoom(string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSpecialtyProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

            return Request.CreateResponse(HttpStatusCode.OK, result.FirstOrDefault());
        }

        // GET api/values/5
        [Route("multipleproceduresdefault")]
        [SwaggerOperation("GetMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public async Task<HttpResponseMessage> GetMultipleProceduresDefaultCardFlowRoom(List<string> cptCodes)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = new List<CardFlowRoom>();

            foreach (var cptCode in cptCodes)
            {
                var procedures = await sqlHelper.GetProcedureDefaultCardFlowRoom(user.ProviderID, user.LocationID, cptCode);

                result.AddRange(procedures);
            }

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [Route("specialtymultipleproceduresdefault")]
        [SwaggerOperation("GetSpecialtyMultipleProceduresDefaultCardFlowRoom")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardFlowRoom>))]
        public async Task<HttpResponseMessage> GetSpecialtyMultipleProceduresDefaultCardFlowRoom(int specialtyId, string cptCodes)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetSpecialtyMultipleProceduresDefaultCardFlowRoom(user.ProviderID, user.LocationID, specialtyId, cptCodes);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("UpdateQuantity")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("updateQuantity", Name = "UpdateQuantity")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateItemQty(int cardId, [FromBody]CardQuantityEdit cardQuantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateCardQuantity(cardId, cardQuantity, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values
        [SwaggerOperation("UpdateQuantityRequest")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("updateQuantityRequest", Name = "UpdateQuantityRequest")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateItemQtyRequest(int cardId, int surgeryId, [FromBody]CardQuantityEditRequest cardQuantity)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            foreach (var editRequest in cardQuantity.EditData)
            {
                if (cardQuantity.Target == "C")
                    await sqlHelper.UpdateCardQuantityRequest(cardId, editRequest, user.ProviderID, user.LocationID);
                else if (editRequest.TrayID.HasValue)
                    await sqlHelper.AddCustomSurgeryTrayItem(surgeryId, editRequest.TrayID.Value, editRequest.ItemID, editRequest.OpenQty ?? 0, user.ProviderID, user.LocationID);
                else
                    await sqlHelper.UpdateSurgeryItemQuantity(surgeryId, editRequest, user.ProviderID, user.LocationID);
            }

            return Ok();
        }

        // POST api/values
        [SwaggerOperation("AssignFlow")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignFlow", Name = "AssignFlowCard")]
        public async Task<HttpResponseMessage> AssignToCard(int cardId, int flowId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignFlowToCard(flowId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        // POST api/values
        [SwaggerOperation("AssignRoomSetup")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("assignRoomSetup", Name = "AssignRoomSetupCard")]
        public async Task<HttpResponseMessage> AssignRoomSetupToCard(int cardId, int roomSetupId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignRoomSetupToCard(roomSetupId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 418);
        }

        [SwaggerOperation("AssignCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("cardUser", Name = "AssignCardUser")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignCardUser(int cardId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.AssignUserToCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        [SwaggerOperation("DeleteCardUser")]
        [SwaggerResponse(HttpStatusCode.Created)]
        [Route("cardUser", Name = "DeleteCardUser")]
        [HttpDelete]
        public async Task<IHttpActionResult> DeleteCardUser(int cardId, int userId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.RemoveUserFromCard(cardId, userId, user.ProviderID, user.LocationID);

            return Ok();
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardItem", Name = "DeleteCardItem")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteCardItem(int cardId, int itemId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteCardItem(cardId, itemId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardItem")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardItem", Name = "AssignCardItem")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutCardItem(int cardId, int itemId, [FromBody]CardItemPost value)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateCardItem(cardId, itemId, value.OpenQty, value.HoldQty, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        // PUT api/values
        [SwaggerOperation("AssignCardProcedure")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        [Route("cardProcedure", Name = "AssignCardProcedure")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> PutCardProcedure(int cardId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values
        [SwaggerOperation("DeleteCardProcedure")]
        [Route("cardProcedure", Name = "DeleteCardProcedure")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> DeleteCardProcedure(int cardId, int procedureId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteCardProcedure(cardId, procedureId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(int))]
        public async Task<HttpResponseMessage> Post([FromBody]CardPost value)
        {
            try
            {
                var user = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();

                var cardCategoryId = await sqlHelper.ParseCardCategory(value.CardCategory, user.ProviderID, user.LocationID);

                var cardId = await sqlHelper.InsertCard(value.Description, value.OwnerUserID,
                    value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                    value.BundleID, value.BundleFlag, cardCategoryId,
                    value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                    user.ProviderID, user.LocationID);

                await InitializeCardProcedures(sqlHelper, cardId, user, value.Procedures);

                await sqlHelper.UpdateCardCategoryXRef(cardId, value.CardCategories, user.ProviderID, user.LocationID);

                return Request.CreateResponse(HttpStatusCode.Created, cardId);
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int id, [FromBody]CardPost value)
        {
            try
            {
                var user = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();

                var cardCategoryId = await sqlHelper.ParseCardCategory(value.CardCategory, user.ProviderID, user.LocationID);

                if (value.Procedures != null)
                {
                    await InitializeCardProcedures(sqlHelper, id, user, value.Procedures);
                }
                else
                {
                    await sqlHelper.UpdateCard(id, value.Description, value.SpecialtyID, value.ProcedureID, value.TemplateFlowID, value.TemplateRoomSetupID,
                        value.BundleID, value.BundleFlag, cardCategoryId,
                        value.DefaultFlag == "1", value.SpecialtyDefaultFlag == "1",
                        user.ProviderID, user.LocationID);
                }

                await sqlHelper.UpdateCardCategoryXRef(id, value.CardCategories, user.ProviderID, user.LocationID);

                return Ok();
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        private async Task InitializeCardProcedures(SqlHelper sqlHelper, int cardId, UserSecurity user, List<CardPostImportProcedure> procedures)
        {
            if (procedures != null)
            {
                await sqlHelper.InitializeCard(cardId, user.ProviderID, user.LocationID);

                foreach (var procedure in procedures)
                {
                    if (!string.IsNullOrEmpty(procedure.ImportSurgeon) && !string.IsNullOrEmpty(procedure.ImportProcedure))
                    {
                        await sqlHelper.InsertCardItemFromStage(cardId, user.ProviderID, user.LocationID,
                            procedure.ImportProcedure, procedure.ImportSurgeon);
                    }

                }
            }
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            try
            {
            var user = await CacheUtil.GetUserSecurity();
                var sqlHelper = new SqlHelper();

                await sqlHelper.DeleteCard(id, user.ProviderID, user.LocationID);

            return Ok();
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }
    }
}