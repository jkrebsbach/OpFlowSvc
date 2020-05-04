using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
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
        [Route("procedureProfile/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetProcedureProfile([FromBody] ProcedureProfileRequestPost request, int? procedureProfileId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            ProcedureProfile profile = null;
            var trayUsage = new List<ProcedureProfileTrayUsage>();
            if (procedureProfileId.HasValue)
            {
                profile = await sqlHelper.GetProcedureProfile(procedureProfileId.Value, request.LocationFilter);
            }

            var cardCategories = await sqlHelper.GetCardCategories();
            var specialties = await sqlHelper.GetSpecialtiesInternal();
            var surgeons = await sqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
            var cards = await sqlHelper.GetCardsInternal(request.LocationFilter);
            var trays = await sqlHelper.GetTraysInternal(request.LocationFilter);
            var proposed = await sqlHelper.GetProposedTraysInternal(request.LocationFilter);
            var itemCategories = await sqlHelper.GetItemCategories(user.ProviderID, user.LocationID);
            var instrumentCategories = await sqlHelper.GetInstrumentCategories(user.ProviderID, user.LocationID);
            var providers = await sqlHelper.GetOpFlowSetup();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProcedureProfile = profile,
                CardCategories = cardCategories,
                Specialties = specialties,
                Cards = cards,
                Trays = trays,
                Proposed = proposed,
                ItemCategories = itemCategories,
                InstrumentCategories = instrumentCategories,
                Surgeons = surgeons,
                Locations = providers.SelectMany(p => p.Locations)
            });
        }

        [SwaggerOperation("GetItems")]
        [Route("items")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetItems([FromBody] ProcedureProfileRequestPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var items = await sqlHelper.GetItemsInternal(request.LocationFilter);

            return Request.CreateResponse(HttpStatusCode.OK, items);
        }

        [SwaggerOperation("GetTrayItems")]
        [Route("trayItems")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ProcedureProfile))]
        [HttpPost]
        public async Task<HttpResponseMessage> GetTrayItems([FromBody] ProcedureProfileRequestPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var items = await sqlHelper.GetTrayItemsInternal(request.TrayID);

            return Request.CreateResponse(HttpStatusCode.OK, items);
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

            var result = await sqlHelper.UpdateProcedureProfile(procedureProfileId, request.ProfileName, request.LocationFilter, request.CardCategoryID, request.SpecialtyID,
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

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var result = "Item,Type,Category,Avg Used,Quantity\r\n";
            foreach (var item in profile.Items)
            {
                result += $"\"{item.ItemDescription.Replace("\"", "\"\"")}\",\"{item.ItemType.Replace("\"", "\"\"")}\",\"{item.Category.Replace("\"", "\"\"")}\",{item.AvgUsed},{item.Quantity}\r\n";
            }

            return ResponseHelper.CsvResponse(result);
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileTrayCsv")]
        [Route("csvTray/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProcedureProfileTrayCsv(int procedureProfileId, int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var result = "Tray,Instrument,Category,Reason,Avg Used, Quantity\r\n";
            foreach (var instrument in profile.TrayItems.OrderBy(ti => ti.Category).ThenBy(ti => ti.ItemDescription))
            {
                var comparables = instrument.ComparableInstruments.Where(ci => ci.LocationID == locationId);

                if (comparables.Any())
                {
                    foreach (var comparable in comparables)
                    {
                        result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{comparable.ItemDescription.Replace("\"", "\"\"")}\",\"{instrument.Category.Replace("\"", "\"\"")}\",\"{instrument.Reason}\",{instrument.AvgUsed},{instrument.Quantity}\r\n";
                    }
                }
                else
                {
                    result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{instrument.ItemDescription.Replace("\"", "\"\"")}\",\"{instrument.Category.Replace("\"", "\"\"")}\",\"{instrument.Reason}\",{instrument.AvgUsed},{instrument.Quantity}\r\n";
                }
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

            var data = await sqlHelper.GetProcedureProfileDashboardComparison(procedureProfileId, 
                request.LocationFilter, request.SpecialtyID, request.CardCategoryID, request.SurgeonID, request.ProposalID);
            var comparisons = data.Comparisons;

            var categories = comparisons.OrderBy(c => c.GroupName).Select(c => c.GroupName).Distinct();

            var results = comparisons.OrderBy(c => c.Category).ThenBy(c => c.Relationship).ThenBy(c => c.InstrumentName).GroupBy(c => c.Category).Select(c =>
                new ProcedureProfileDashboardCategoryGroup()
                {
                    CategoryName = c.Key,
                    Groups = c.GroupBy(i => new { i.TrayName, i.InstrumentName, i.Relationship }).Select(i =>
                    new ProcedureProfileDashboardGroup()
                    {
                        TrayName = i.Key.TrayName,
                        InstrumentName = i.Key.InstrumentName,
                        Relationship = i.Key.Relationship,
                        Results = ProcedureProfileDashboardComparison.Summarize(categories, i.ToList())
                    }).ToList()
                });
            


            return Request.CreateResponse(HttpStatusCode.OK, new {
                Categories = categories,
                Results = results
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

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, request.LocationFilter);

            var traySummary = profile.TrayItems.GroupBy(p => p.TrayName).Select(p => new ProfileItem()
            {
                ItemType = "TRAY",
                ItemDescription = p.Key,
                Quantity = p.Sum(ti => ti.Quantity)
            });


            var profileItems = profile.Items.Union(traySummary);
            List<CardItem> studyItems = new List<CardItem>();


            var cardItems = await sqlHelper.GetCardItemsInternal(request.Cards);
            studyItems.AddRange(cardItems);
            
            var trayItems = await sqlHelper.GetTrayItemsInternal(request.Trays);
            if (trayItems.Any())
            {
                studyItems.Add(new CardItem()
                {
                    Category = "TRAY",
                    ItemDescription = trayItems.First().TrayName,
                    Quantity = trayItems.Sum(ti => ti.Quantity)
                });
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
            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, request.LocationFilter);
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
        public async Task<HttpResponseMessage> PutProcedureProfileDashboard(int procedureProfileId, [FromBody] ProcedureProfileDashboardPost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileDashboard(procedureProfileId, request.LocationFilter, request.Cards, request.Trays, request.Proposals,
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
        public async Task<HttpResponseMessage> PutProcedureProfileTray(int procedureProfileId, int trayItemId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, user.ProviderID, user.LocationID);

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
        [SwaggerOperation("PutTrayImportCsv")]
        [Route("trayImportCsv")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTrayImportCsv(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            int? logId = null;

            var importTypeId = 3;

            try
            {
                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");
                var fileName = fileNameParam?.Value.Trim('"') ?? "";
                var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

                var fileParser = new FileParser(fileName, fileContents);

                await fileParser.ParseFile(sqlHelper, importTypeId, user.ProviderID, user.LocationID);

                var trayIds = new List<int>();

                foreach (var record in fileParser.Records)
                {
                    var result = await sqlHelper.InsertStagingData(user.ProviderID, user.LocationID, null, record, fileParser.Relations);
                    foreach (var message in result.Messages)
                    {
                        await sqlHelper.InsertImportMessage(user.ProviderID, user.LocationID, logId.Value, "WARN", message,
                            null, null);
                    }

                    trayIds.Add(result.Identity);
                }

                foreach (var trayItemId in trayIds.Distinct())
                {
                    await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId,
                        trayItemId, user.ProviderID, user.LocationID);
                }

                return Request.CreateResponse(HttpStatusCode.OK, fileParser.Status);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                if (logId != null)
                    await sqlHelper.InsertImportMessage(user.ProviderID, user.LocationID, logId.Value, "ERROR", ex.Message, null, null);

                throw;
            }
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