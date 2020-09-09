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
using System.Text;
using System.Threading.Tasks;
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

            var profiles = await sqlHelper.GetProcedureProfiles();
            var providers = await sqlHelper.GetOpFlowSetup();
            var specialties = await sqlHelper.GetSpecialtyMaster();
            var cardCategories = await sqlHelper.GetCardCategories();
            var surgeons = await sqlHelper.GetSurgeonsProcedureProfile();
            var procedures = await sqlHelper.GetProcedures(null, 1, 1);
            var categories = await sqlHelper.GetProcedureProfileCategories();

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Profiles = profiles,
                CardCategories = cardCategories,
                Specialties = specialties,
                Providers = providers,
                Locations = providers.SelectMany(p => p.Locations),
                Surgeons = surgeons,
                Procedures = procedures,
                ProcedureProfileCategories = categories
            });
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

            var cards = await sqlHelper.GetCardsInternal(request.LocationFilter);
            var trays = await sqlHelper.GetTraysInternal(request.LocationFilter);
            var proposed = await sqlHelper.GetProposedTraysInternal(request.LocationFilter);
            var itemCategories = await sqlHelper.GetItemCategories(user.ProviderID, user.LocationID);
            var instrumentCategories = await sqlHelper.GetInstrumentCategories(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProcedureProfile = profile,
                Cards = cards,
                Trays = trays,
                Proposed = proposed,
                ItemCategories = itemCategories,
                InstrumentCategories = instrumentCategories
            });
        }

        // GET api/surgery?userId=5
        [SwaggerOperation("GetSurgeonPreferences")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<SurgeonPreference>))]
        [Route("surgeonPreferences")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSurgeonPreferences(int cardId, [FromBody] SurgeonPreferencesFilter request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var card = (await sqlHelper.GetCardData(cardId, user.SelectedLocation)).FirstOrDefault();

            if (card == null) return Request.CreateResponse(HttpStatusCode.NotFound);

            var procedureProfile = await sqlHelper.GetProcedureProfileByCard(cardId, user.SelectedLocation);
            var surgeon = await sqlHelper.GetUser(user.SelectedLocation, card.OwnerUserID);

            if (procedureProfile == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    ProcedureProfile = procedureProfile,
                    Surgeon = surgeon,
                    Card = card,
                    SurgeonPreferences = new SurgeonUsagePreferenceSummary(),
                    ProcedureProfileMetrics = new List<ProcedureProfileMetric>(),
                    Variances = new List<RawSurgeonUsageVariant>()
                });

            }

            var preferences = await sqlHelper.GetSurgeonUsagePreferences(procedureProfile.ProcedureProfileID, card.OwnerUserID, 
                request.Answers, user.SelectedLocation);

            var variances = await sqlHelper.GetSurgeonUsagePreferenceVariants(procedureProfile.ProcedureProfileID);

            var profileMetrics = await sqlHelper.GetProcedureProfileMetrics("PROC", user.LocationID);
            var patientMetrics = await sqlHelper.GetProcedureProfileMetrics("PAT", user.LocationID);

            profileMetrics.AddRange(patientMetrics);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProcedureProfile = procedureProfile,
                Surgeon = surgeon,
                Card = card,
                SurgeonPreferences = preferences,
                ProcedureProfileMetrics = profileMetrics,
                Variances = variances
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetVendorSupply")]
        [Route("vendorSupply/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardWithCategory>))]
        public async Task<HttpResponseMessage> GetVendorSupply(int procedureProfileId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && !user.Vendor)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, null);

            return Request.CreateResponse(HttpStatusCode.OK, profile);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardCategories")]
        [Route("cardCategories")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardWithCategory>))]
        public async Task<HttpResponseMessage> GetCardCategories(int? specialtyId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var cards = await sqlHelper.GetCardCategoryXRef(null, specialtyId, null, null, null, 1, 1);
            var cardCategories = cards.SelectMany(c => c.CardCategories ?? new List<CardCategoryXRef>()).GroupBy(c => new { c.CardCategoryID, c.CardCategory })
                .Select(c => new CardCategory()
                {
                    CardCategoryID = c.Key.CardCategoryID,
                    CategoryName = c.Key.CardCategory
                });
            
            return Request.CreateResponse(HttpStatusCode.OK, cardCategories);
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

            var result = "Tray,Instrument,Comparable Tray,Comparable Instrument,Category,Reason,Avg Used, Original Quantity, Proposed Quantity\r\n";
            foreach (var instrument in profile.TrayItems.OrderBy(ti => ti.Category).ThenBy(ti => ti.ItemDescription))
            {
                var comparables = instrument.ComparableInstruments.Where(ci => ci.LocationID == locationId);

                if (comparables.Any())
                {
                    foreach (var comparable in comparables)
                    {
                        result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{instrument.ItemDescription.Replace("\"", "\"\"")}\",\"{comparable.TrayName.Replace("\"", "\"\"")}\",\"{comparable.ItemDescription.Replace("\"", "\"\"")}\",\"{instrument.Category.Replace("\"", "\"\"")}\",\"{instrument.Reason}\",{instrument.AvgUsed},{comparable.Quantity},{instrument.Quantity}\r\n";
                    }
                }
                else
                {
                    result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{instrument.ItemDescription.Replace("\"", "\"\"")}\",,,\"{instrument.Category.Replace("\"", "\"\"")}\",\"{instrument.Reason}\",{instrument.AvgUsed},,{instrument.Quantity}\r\n";
                }
            }

            return ResponseHelper.CsvResponse(result);
        }

        // GET api/values/5
        [SwaggerOperation("GetUnmappedProcedureProfileTrayCsv")]
        [Route("csvUnmapped/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetUnmappedProcedureProfileTrayCsv(int procedureProfileId, int locationId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var comparableTrays = new List<int>();


            foreach (var instrument in profile.TrayItems.OrderBy(ti => ti.Category).ThenBy(ti => ti.ItemDescription))
            {
                var comparables = instrument.ComparableInstruments.Where(ci => ci.LocationID == locationId);
                comparableTrays.AddRange(comparables.Select(c => c.RelatedTrayItemID));
            }

            var result = "Comparable Tray,Unmapped Instrument, Quantity\r\n";
            var trayInstruments = await sqlHelper.GetTrayItemsInternal(comparableTrays);
            foreach (var instrument in trayInstruments.OrderBy(ti => ti.TrayName).ThenBy(ti => ti.InstrumentName))
            {
                if (!profile.TrayItems.Any(ti => ti.ComparableInstruments.Any(i => i.RelatedInstrumentID == instrument.InstrumentID)))
                {
                    result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{instrument.ItemDescription.Replace("\"", "\"\"")}\",{instrument.Quantity}\r\n";
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

            IEnumerable<ProcedureProfileDashboardCategoryGroup> results;

            if (request.Group == "tray")
            {
                results = comparisons.OrderBy(c => c.TrayName).OrderBy(c => c.Category).ThenBy(c => c.Relationship).ThenBy(c => c.InstrumentName).GroupBy(c => new { c.TrayName, c.Category }).Select(c =>
                   new ProcedureProfileDashboardCategoryGroup()
                   {
                       CategoryName = c.Key.TrayName + " - " + c.Key.Category,
                       Groups = c.GroupBy(i => new { i.TrayName, i.InstrumentName, i.Relationship }).Select(i =>
                       new ProcedureProfileDashboardGroup()
                       {
                           TrayName = i.Key.TrayName,
                           InstrumentName = i.Key.InstrumentName,
                           Relationship = i.Key.Relationship,
                           Results = ProcedureProfileDashboardComparison.Summarize(categories, i.ToList())
                       }).ToList()
                   });
            } else
            {
                results = comparisons.OrderBy(c => c.Category).ThenBy(c => c.Relationship).ThenBy(c => c.InstrumentName).GroupBy(c => c.Category).Select(c =>
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
            }            


            return Request.CreateResponse(HttpStatusCode.OK, new {
                Categories = categories,
                Results = results
            });
        }

        // GET api/values/5
        [SwaggerOperation("PutTrayRequest")]
        [Route("trayRequest/{procedureProfileId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfileCardComparison>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutTrayRequest(int procedureProfileId, [FromBody] TrayRequestEmailRequest request)
        {
            var sqlHelper = new SqlHelper();
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var procedureProfile = await sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var targets = request.Emails.Select(e => new User() { Email = e });
            var message = await GenerateTrayRequestMessage(procedureProfile);
            var attachments = await GenerateTrayRequestAttachments(procedureProfile);

            await EmailHelper.SendEmail(targets, "Tray Request Summary", message, attachments);

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }

        private async Task<string> GenerateTrayRequestMessage(ProcedureProfile procedureProfile)
        {
            var sqlHelper = new SqlHelper();
            var result = string.Empty;

            result += $"{procedureProfile.ProcedureProfileName} Summary\r\n--------\r\n";

            result += "<table><thead><tr><th>Location</th>" +
                "<th>Date</th>" +
                "<th>Surgeon</th>" +
                "<th>Procedure</th>" +
                "<th>CPT</th>" +
                "<th>Questions</th></tr></thead><tbody>";

            var profileSurgeries = await sqlHelper.GetProcedureProfileSurgeries(procedureProfile.ProcedureProfileID);
            foreach (var profileSurgery in profileSurgeries)
            {
                result += $"<tr><td>{profileSurgery.Location}</td>" +
                    $"<td>{profileSurgery.SurgeryDateTime}</td>" +
                    $"<td>{profileSurgery.Surgeon}</td>" +
                    $"<td>{profileSurgery.Procedure}</td>" +
                    $"<td>{profileSurgery.CPTCodes}</td>" +
                    $"<td>{profileSurgery.Questions}</td></tr>";
            }

            result += "</tbody></table>";

            foreach (var card in procedureProfile.Cards)
            {
                var cardItems = await sqlHelper.GetCardItems(card.CardID, card.LocationID);

                result += $"Card - {card.CardDescription}\r\n--------\r\n";
                foreach (var item in cardItems.Where(c => c.ItemType == "TRAY"))
                {
                    result += $"Tray - {item.ItemDescription}\r\n";
                }
            }

            return result;
        }

        private async Task<List<EmailHelper.MessageAttachment>> GenerateTrayRequestAttachments(ProcedureProfile procedureProfile)
        {
            var sqlHelper = new SqlHelper();
            var result = new List<EmailHelper.MessageAttachment>();

            foreach (var card in procedureProfile.Cards)
            {
                var cardItems = await sqlHelper.GetCardItems(card.CardID, card.LocationID);

                var csvData = "Tray, Item, Quantity\r\n";

                foreach (var item in cardItems)
                {
                    if (item.ItemType == "TRAY")
                    {
                        var trayItems = await sqlHelper.GetTrayItems(item.ItemID, card.LocationID);
                        foreach (var trayItem in trayItems)
                        {
                            csvData += $"{item.ItemDescription},\"{trayItem.InstrumentName}\", {trayItem.Quantity}\r\n";
                        }
                    }
                    else
                    {
                        csvData += $"No Tray,\"{item.ItemDescription}\", {item.Quantity}\r\n";
                    }
                }

                result.Add(new EmailHelper.MessageAttachment()
                {
                    Filename = $"{card.CardDescription}_CardExport.csv",
                    FileContent = Encoding.ASCII.GetBytes(csvData)
                });
            }

            return result;
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

            var profileItems = profile.Items.Union(profile.TrayItems).Union(traySummary);
            List<CardItem> studyItems = new List<CardItem>();


            var cardItems = await sqlHelper.GetCardItemsInternal(request.Cards, request.CardCategories);
            studyItems.AddRange(cardItems);
            
            var trayItems = await sqlHelper.GetTrayItemsInternal(request.Trays);
            if (trayItems.Any())
            {
                studyItems.Add(new CardItem()
                {
                    Category = "TRAY",
                    CardName = "No Card",
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

        // GET api/values/5
        [SwaggerOperation("FilterCasePreferencesOpp")]
        [Route("filterCasePreferencesOpp")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<HttpResponseMessage> FilterCasePreferencesOpp([FromBody]ProcedureProfileCasePreferenceFilterRequest request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var profiles = await sqlHelper.GetProcedureProfiles();
            var proposals = await sqlHelper.GetProposedTraysInternal(request.LocationFilter);
            var trays = await sqlHelper.GetTraysInternal(request.LocationFilter);

            if (request.SpecialtyID.HasValue)
            {
                profiles = profiles.Where(p => p.Specialties.Any(s => s.SpecialtyID == request.SpecialtyID.Value)).ToList();
            }

            if ((request.CardCategoryID?.Count() ?? 0) > 0)
                profiles = profiles.Where(p => p.CardCategories.Any(cc => request.CardCategoryID.Contains(cc.CardCategoryID))).ToList();


            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ProcedureProfiles = profiles,
                Proposals = proposals,
                Trays = trays
            });
        }

        // GET api/values/5
        [SwaggerOperation("ExecuteCasePreferencesOpp")]
        [Route("executeCasePreferencesOpp")]
        [Route("executeCasePreferencesOpp/{format}")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPut]
        [HttpPost]
        public async Task<HttpResponseMessage> ExecuteCasePreferencesOpp([FromBody]ProcedureProfileCasePreferenceReportRequest request, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            if (request.ProcedureProfileID?.Any() != true)
                return Request.CreateResponse(HttpStatusCode.OK, -1);

            var profile = await sqlHelper.GetProcedureProfileCasePreferencesReport(request.LocationFilter, request.ProcedureProfileID, request.ProposalID, request.TrayID);
            
            if (format == "xlsx")
            {
                var workbook = ExcelHelper.GenerateWorkbook(profile);
                return ResponseHelper.ExcelResponse(workbook);
            }
            if (format == "email")
            {
                var workbook = ExcelHelper.GenerateWorkbook(profile);
                var emailTarget = new User()
                {
                    Email = request.Email,
                    FirstName = request.Email
                };

                var attachments = new List<EmailHelper.MessageAttachment>()
                {
                    new EmailHelper.MessageAttachment()
                    {
                        Filename = "CasePreferences.xlsx",
                        FileContent = workbook
                    }
                };

                await EmailHelper.SendEmail(emailTarget, "Case Preference OPP Report", "Requested Case Preference OPP Report", attachments);
            }

            return Request.CreateResponse(HttpStatusCode.OK, profile);
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

            var profile = await sqlHelper.GetProcedureProfile(procedureProfileId, request.LocationFilter);
            var studyItems = await sqlHelper.GetCardItemsInternal(request.Cards, null);
            var trayItems = await sqlHelper.GetTrayItemsInternal(request.Trays);

            studyItems.AddRange(trayItems.Select(item => new CardItem()
            {
                TrayName = item.TrayName,
                TrayID = item.TrayItemID,
                Quantity = item.Quantity,
                UnitCost = item.InstrumentCost,
                ItemDescription = item.ItemDescription,
                AvgUsed = item.AvgUsed
            }));

            studyItems.RemoveAll(s => s.TrayID == null);

            var profileTrayItems = profile.TrayItems;
            var shared = profileTrayItems.Where(t => studyItems.Any(ci => ci.ItemID == t.ItemID)).ToList();

            foreach (var sharedItem in shared)
            {
                profileTrayItems.RemoveAll(p => p.ItemID == sharedItem.ItemID);
                studyItems.RemoveAll(p => p.ItemID == sharedItem.ItemID);
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
                Card = studyItems.GroupBy(c => c.TrayName).Select(c =>
                new
                {
                    TrayName = c.Key ?? "ITEMS",
                    Instruments = c.ToList()
                })
            });
        }

        // GET api/values/5
        [SwaggerOperation("GetProcedureProfileCategories")]
        [Route("category")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetProcedureProfileCategories()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.GetProcedureProfileCategories();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("UpdateProcedureProfileCategories")]
        [Route("category")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateProcedureProfileCategories(int? categoryId, string categoryName)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileCategoryList(categoryId, categoryName);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostProcedureProfileUpdate")]
        [Route("procedureProfileUpdate")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostProcedureProfileUpdate(int procedureProfileId, [FromBody] ProcedureProfileCategoryPost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileCategory(procedureProfileId, request.Name, request.CategoryID, request.OwnerID,
                user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
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
        [Route("procedureProfileStep")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileStep(int procedureProfileId, int stepId, int duration)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileStep(procedureProfileId, stepId, duration, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileCpt")]
        [Route("procedureProfileStep")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureProfileStep(int procedureProfileId, int stepId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileStep(procedureProfileId, stepId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostProcedureProfileSteps")]
        [Route("procedureProfileStep")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPost]
        public async Task<HttpResponseMessage> PostProcedureProfileSteps(int procedureProfileId, [FromBody] ProcedureProfileStepUpdate request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal")
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileSteps(procedureProfileId, request.Steps, user.ProviderID, user.LocationID);

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

            if (user.RoleType != "Internal" && user.Vendor == false)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProcedureProfileTrayGroup")]
        [Route("procedureProfileTrayGroup")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardCategory>))]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureProfileTrayGroup(int procedureProfileId, int trayGroupId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.Vendor == false)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            var trayGroups = await sqlHelper.GetTrayGroups(user.SelectedLocation);
            var trayGroup = trayGroups.First(t => t.TrayGroupID == trayGroupId);

            await sqlHelper.InsertProcedureProfileTrayGroup(procedureProfileId, trayGroupId, user.SelectedLocation);

            var result = -1;
            foreach (var tray in trayGroup.Trays)
            {
                result = await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, tray.TrayItemID, user.SelectedLocation);
            }

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
        [SwaggerOperation("DeleteProcedureProfileTray")]
        [Route("procedureProfileTray")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureProfileTray(int procedureProfileId, int trayId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.Vendor == false)
                return Request.CreateResponse(HttpStatusCode.NotFound);
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteProcedureProfileTray(procedureProfileId, trayId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, trayId);
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

                await fileParser.ParseFile(sqlHelper, importTypeId, user.SelectedLocation);

                var trayIds = new List<int>();

                foreach (var record in fileParser.Records)
                {
                    var result = await sqlHelper.InsertStagingData(user.SelectedLocation, null, record, fileParser.Relations);
                    foreach (var message in result.Messages)
                    {
                        await sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message,
                            null, null);
                    }

                    trayIds.Add(result.Identity);
                }

                foreach (var trayItemId in trayIds.Distinct())
                {
                    await sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, user.SelectedLocation);
                }

                return Request.CreateResponse(HttpStatusCode.OK, fileParser.Status);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                if (logId != null)
                    await sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "ERROR", ex.Message, null, null);

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

            var result = await sqlHelper.GetCptCodes();

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}