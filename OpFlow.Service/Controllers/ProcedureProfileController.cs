using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/procedureProfile")]
    public class ProcedureProfileController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfiles")]
        [Route("procedureProfiles")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfile>))]
        public async Task<HttpResponseMessage> GetProcedureProfiles()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetProcedureProfiles();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfile")]
        [Route("procedureProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        public async Task<HttpResponseMessage> GetProcedureProfile(int? procedureProfileId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            ProcedureProfile profile = null;
            var trayUsage = new List<ProcedureProfileTrayUsage>();
            if (procedureProfileId.HasValue)
            {
                profile = await sqlHelper.GetProcedureProfile(procedureProfileId.Value);
                
                trayUsage = await sqlHelper.GetProcedureProfileTrayUsage(procedureProfileId.Value, user.ProviderID, user.LocationID);
            }

            var cardCategories = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);
            var specialties = await sqlHelper.GetSpecialtiesInternal();
            var surgeons = await sqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
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
                InstrumentCategories = instrumentCategories,
                Surgeons = surgeons
            });
        }

        // GET api/values/5
        [SwaggerOperation("PostProcedureProfile")]
        [Route("procedureProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostProcedureProfile(int? procedureProfileId, [FromBody] ProcedureProfilePost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfile(procedureProfileId, request.ProfileName, request.CardCategoryID, request.SpecialtyID,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileItemCsv")]
        [Route("csvItem/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProcedureProfileItemCsv(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId);

            var result = "Item,Type,Category,Avg Used,Quantity\r\n";
            foreach (var item in profile.Items)
            {
                result += $"\"{item.ItemDescription}\",\"{item.ItemType}\",\"{item.Category}\",{item.AvgUsed},{item.Quantity}\r\n";
            }

            return ResponseHelper.CsvResponse(result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileTrayCsv")]
        [Route("csvTray/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProcedureProfileTrayCsv(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId);

            var result = "Tray,Instrument,Category,Reason,Avg Used, Quantity\r\n";
            foreach (var tray in profile.TrayItems)
            {
                result += $"\"{tray.TrayName}\",\"{tray.ItemDescription}\",\"{tray.Category}\",\"{tray.Reason}\",{tray.AvgUsed},{tray.Quantity}\r\n";
            }

            return ResponseHelper.CsvResponse(result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileChart")]
        [Route("procedureProfileChart/{procedureProfileId}")]
        [Route("procedureProfileChart/{format}/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        [HttpPut]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfileChart(int procedureProfileId, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            format = format ?? "IMAGE";

            var analytics = await sqlHelper.GetProcedureProfileReport(procedureProfileId, user.ProviderID, user.LocationID);

            var profile = analytics.Tables[0].DefaultView;
            var datasets = new Dictionary<string, DataTable>
            {
                ["ProcedureProfile"] = profile.ToTable()
            };

            var reportName = "ProcedureProfile";

            var result = ReportHelper.GetReport(reportName, format, datasets);

            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.ImageResponse(Request, webImage);
            }
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileTrayAlignment")]
        [Route("procedureProfileTrayAlignment/{procedureProfileId}")]
        [Route("procedureProfileTrayAlignment/{format}/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        [HttpPut]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfileTrayAlignment([FromBody] ProcedureProfileAlignmentPost post, int procedureProfileId, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            format = format ?? "IMAGE";

            var analytics = await sqlHelper.GetProcedureProfileTrayAlignment(procedureProfileId, post.CardId, post.TrayId, user.ProviderID, user.LocationID);

            var profile = analytics.Tables[0].DefaultView;
            switch (post.OrderBy)
            {
                case "HIGH":
                    profile.Sort = "AVG_Qty DESC";
                    break;
                case "LOW":
                    profile.Sort = "AVG_Qty ASC";
                    break;
                case "VAR":
                    profile.Sort = "VAR_Qty DESC";
                    break;
                case "OPP":
                default:
                    profile.Sort = "OPP_Qty DESC";
                    break;
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["Alignment"] = profile.ToTable()
            };

            var reportName = "ProfileAlignment";

            var result = ReportHelper.GetReport(reportName, format, datasets);
            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                var summary = analytics.Tables[1].DataTableToList<ProcedureProfileAnalyticsSummary>();

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
            }
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileSupplyAlignment")]
        [Route("procedureProfileSupplyAlignment/{procedureProfileId}")]
        [Route("procedureProfileSupplyAlignment/{format}/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        [HttpPut]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfileSupplyAlignment([FromBody] ProcedureProfileAlignmentPost post, int procedureProfileId, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            format = format ?? "IMAGE";

            var analytics = await sqlHelper.GetProcedureProfileSupplyAlignment(procedureProfileId, post.CardId, post.TrayId, user.ProviderID, user.LocationID);

            var profile = analytics.Tables[0].DefaultView;
            switch (post.OrderBy)
            {
                case "HIGH":
                    profile.Sort = "AVG_Qty DESC";
                    break;
                case "LOW":
                    profile.Sort = "AVG_Qty ASC";
                    break;
                case "VAR":
                    profile.Sort = "VAR_Qty DESC";
                    break;
                case "OPP":
                default:
                    profile.Sort = "OPP_Qty DESC";
                    break;
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["Alignment"] = profile.ToTable()
            };

            var reportName = "ProfileAlignment";

            var result = ReportHelper.GetReport(reportName, format, datasets);
            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                var summary = analytics.Tables[1].DataTableToList<ProcedureProfileAnalyticsSummary>();

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
            }
        }

        // GET api/values/5
        [SwaggerOperation("PostDashboardComparison")]
        [Route("dashboardComparison/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfileCardComparison>))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostDashboardComparison(int procedureProfileId, [FromBody]ProcedureProfileDashboardComparisonRequest request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var comparisons = await sqlHelper.GetProcedureProfileDashboardComparison(procedureProfileId, request.SpecialtyID, request.CardCategoryID, request.SurgeonID);

            var categories = comparisons.OrderBy(c => c.GroupName).Select(c => c.GroupName).Distinct();

            var groups = comparisons.GroupBy(c => new { c.TrayName, c.InstrumentName }).Select(c =>
                new ProcedureProfileDashboardGroup()
                {
                    TrayName = c.Key.TrayName,
                    InstrumentName = c.Key.InstrumentName,
                    Results = ProcedureProfileDashboardComparison.Summarize(categories, c.ToList())
                });


            return Request.CreateResponse(HttpStatusCode.OK, new {
                Categories = categories,
                Groups = groups
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileCompare")]
        [Route("procedureProfileCompare")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfileCardComparison>))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfileCompare(int procedureProfileId, [FromBody]ProcedureProfileCardComparisonRequest request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId);

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

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var cardItems = new List<CardItem>();
            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId);
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

            foreach (var sharedItem in shared)
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
        [SwaggerOperation("PutProcedureProfileDashboard")]
        [Route("procedureProfileDashboard")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> PutProcedureProfileDashboard(int? procedureProfileId, [FromBody] ProcedureProfileDashboardPost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileDashboard(procedureProfileId, request.Cards, request.Trays, request.Proposals,
                user.ProviderID, user.LocationID);

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

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
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

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, trayType, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileCard")]
        [Route("procedureProfileCard")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileCard(int procedureProfileId, int cardId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateProcedureProfileCard(procedureProfileId, cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, cardId);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileItem")]
        [Route("procedureProfileItem")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileItem(int procedureProfileId, [FromBody] ProcedureProfileItemUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            foreach (var item in post.Items)
            {
                if (item.ItemType == "I") // Instrument
                    await sqlHelper.UpdateProcedureProfileItem(procedureProfileId, null, item.ItemID, item.Category, item.Quantity, user.ProviderID, user.LocationID);
                else // Supply
                    await sqlHelper.UpdateProcedureProfileItem(procedureProfileId, item.ItemID, null, item.Category, item.Quantity, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }

        // GET api/values/5
        [SwaggerOperation("PostProcedureProfileTrayInstrument")]
        [Route("procedureProfileTrayInstrument")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostProcedureProfileTrayInstrument(int procedureProfileId, [FromBody] ProcedureProfileItemUpdatePost post)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            foreach (var item in post.Items)
            {
                await sqlHelper.UpdateProcedureProfileTrayInstrument(procedureProfileId, item.ItemID, item.TrayItemID, 
                    item.CategoryID, item.Quantity, item.Reason, user.ProviderID, user.LocationID);
            }

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteProcedureProfileCpt")]
        [Route("procedureProfileCpt")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureProfileCpt(int procedureProfileId, string cptCode)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
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

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
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
        public async Task<HttpResponseMessage> DeleteProcedureProfileTrayInstrument(int procedureProfileId, int itemId, int trayItemId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteProcedureProfileTrayInstrument(procedureProfileId, itemId, trayItemId, user.ProviderID, user.LocationID);

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
    }
}