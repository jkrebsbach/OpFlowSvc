using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/procedureProfile")]
    public class ProcedureProfileController : OpFlowController
    {
        private EmailHelper _emailHelper;

        public ProcedureProfileController(
            EmailHelper emailHelper,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
            _emailHelper = emailHelper;
        }

        // GET api/values/5
        [Route("procedureProfiles")]
        public async Task<ActionResult> GetProcedureProfiles()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var profiles = await _sqlHelper.GetProcedureProfiles();
            var providers = await _sqlHelper.GetOpFlowSetup();
            var specialties = await _sqlHelper.GetSpecialtyMaster();
            var cardCategories = await _sqlHelper.GetCardCategories();
            var surgeons = await _sqlHelper.GetSurgeonsProcedureProfile();
            var procedures = await _sqlHelper.GetProcedures(null, 1);
            var categories = await _sqlHelper.GetProcedureProfileCategories();

            return Ok(new
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
        [Route("procedureProfile")]
        [Route("procedureProfile/{procedureProfileId}")]
        [HttpPost]
        public async Task<ActionResult> GetProcedureProfile([FromBody] ProcedureProfileRequestPost request, int? procedureProfileId = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            ProcedureProfile profile = null;
            var trayUsage = new List<ProcedureProfileTrayUsage>();
            if (procedureProfileId.HasValue)
            {
                profile = await _sqlHelper.GetProcedureProfile(procedureProfileId.Value, request.LocationFilter);
            }

            var proposed = await _sqlHelper.GetProposedTraysInternal(request.LocationFilter);
            var itemCategories = await _sqlHelper.GetItemCategories(user.SelectedLocation);
            var instrumentCategories = await _sqlHelper.GetInstrumentCategories(user.SelectedLocation);

            return Ok(new
            {
                ProcedureProfile = profile,
                Proposed = proposed,
                ItemCategories = itemCategories,
                InstrumentCategories = instrumentCategories
            });
        }

        // GET api/surgery?userId=5
        [Route("surgeonPreferences")]
        [HttpPost]
        public async Task<ActionResult> PostSurgeonPreferences(int cardId, [FromBody] SurgeonPreferencesFilter request)
        {
            var user = await GetUserSecurity();
            

            var card = (await _sqlHelper.GetCardData(cardId, user.SelectedLocation)).FirstOrDefault();

            if (card == null) return NotFound();

            var procedureProfile = await _sqlHelper.GetProcedureProfileByCard(cardId, user.SelectedLocation);
            var surgeon = await _sqlHelper.GetUser(user.SelectedLocation, card.OwnerUserID);

            if (procedureProfile == null)
            {
                return Ok(new
                {
                    ProcedureProfile = procedureProfile,
                    Surgeon = surgeon,
                    Card = card,
                    SurgeonPreferences = new SurgeonUsagePreferenceSummary(),
                    ProcedureProfileMetrics = new List<ProcedureProfileMetric>(),
                    Variances = new List<RawSurgeonUsageVariant>()
                });

            }

            var preferences = await _sqlHelper.GetSurgeonUsagePreferences(procedureProfile.ProcedureProfileID, card.OwnerUserID, 
                request.Answers, user.SelectedLocation);

            var variances = await _sqlHelper.GetSurgeonUsagePreferenceVariants(procedureProfile.ProcedureProfileID);

            var profileMetrics = await _sqlHelper.GetProcedureProfileMetrics("PROC", user.SelectedLocation);
            var patientMetrics = await _sqlHelper.GetProcedureProfileMetrics("PAT", user.SelectedLocation);

            profileMetrics.AddRange(patientMetrics);

            return Ok(new
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
        [Route("vendorSupply/{procedureProfileId}")]
        public async Task<ActionResult> GetVendorSupply(int procedureProfileId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && !user.Vendor)
                return NotFound();

            

            var profile = await _sqlHelper.GetProcedureProfile(procedureProfileId, null);

            return Ok(profile);
        }

        // GET api/values/5
        [Route("cardCategories")]
        public async Task<ActionResult> GetCardCategories(int? specialtyId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var cards = await _sqlHelper.GetCardCategoryXRef(null, specialtyId, null, null, null, 1);
            var cardCategories = cards.SelectMany(c => c.CardCategories ?? new List<CardCategoryXRef>()).GroupBy(c => new { c.CardCategoryID, c.CardCategory })
                .Select(c => new CardCategory()
                {
                    CardCategoryID = c.Key.CardCategoryID,
                    CategoryName = c.Key.CardCategory
                });
            
            return Ok(cardCategories);
        }

        [Route("items")]
        [HttpPost]
        public async Task<ActionResult> GetItems([FromBody] ProcedureProfileRequestPost request)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var items = await _sqlHelper.GetItemsInternal(request.LocationFilter);

            return Ok(items);
        }

        [Route("trayItems")]
        [HttpPost]
        public async Task<ActionResult> GetTrayItems([FromBody] ProcedureProfileRequestPost request)
        {
            var user = await GetUserSecurity();
            

            if (user.RoleType != "Internal")
                return NotFound();

            var items = await _sqlHelper.GetTrayItemsInternal(request.TrayID);

            return Ok(items);
        }

        // GET api/values/5
        [Route("procedureProfile")]
        [HttpPost]
        public async Task<ActionResult> PostProcedureProfile(int? procedureProfileId, [FromBody] ProcedureProfilePost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.UpdateProcedureProfile(procedureProfileId, request.ProfileName, request.LocationFilter, 
                request.CardCategoryID, request.SpecialtyID, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfile")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfile(int procedureProfileId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
        
            

            var result = await _sqlHelper.DeleteProcedureProfile(procedureProfileId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("csvItem/{procedureProfileId}")]
        [HttpGet]
        public async Task<ActionResult> GetProcedureProfileItemCsv(int procedureProfileId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var profile = await _sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var result = "Item,Type,Category,Avg Used,Quantity\r\n";
            foreach (var item in profile.Items)
            {
                result += $"\"{item.ItemDescription.Replace("\"", "\"\"")}\",\"{item.ItemType.Replace("\"", "\"\"")}\",\"{item.Category.Replace("\"", "\"\"")}\",{item.AvgUsed},{item.Quantity}\r\n";
            }

            return CsvResponse(result);
        }

        // GET api/values/5
        [Route("csvTray/{procedureProfileId}")]
        [HttpGet]
        public async Task<ActionResult> GetProcedureProfileTrayCsv(int procedureProfileId, int locationId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var profile = await _sqlHelper.GetProcedureProfile(procedureProfileId, null);

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

            return CsvResponse(result);
        }

        // GET api/values/5
        [Route("csvUnmapped/{procedureProfileId}")]
        [HttpGet]
        public async Task<ActionResult> GetUnmappedProcedureProfileTrayCsv(int procedureProfileId, int locationId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var profile = await _sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var comparableTrays = new List<int>();


            foreach (var instrument in profile.TrayItems.OrderBy(ti => ti.Category).ThenBy(ti => ti.ItemDescription))
            {
                var comparables = instrument.ComparableInstruments.Where(ci => ci.LocationID == locationId);
                comparableTrays.AddRange(comparables.Select(c => c.RelatedTrayItemID));
            }

            var result = "Comparable Tray,Unmapped Instrument, Quantity\r\n";
            var trayInstruments = await _sqlHelper.GetTrayItemsInternal(comparableTrays);
            foreach (var instrument in trayInstruments.OrderBy(ti => ti.TrayName).ThenBy(ti => ti.InstrumentName))
            {
                if (!profile.TrayItems.Any(ti => ti.ComparableInstruments.Any(i => i.RelatedInstrumentID == instrument.InstrumentID)))
                {
                    result += $"\"{instrument.TrayName.Replace("\"", "\"\"")}\",\"{instrument.ItemDescription.Replace("\"", "\"\"")}\",{instrument.Quantity}\r\n";
                }
            }
            
            return CsvResponse(result);
        }

        // GET api/values/5
        [Route("procedureProfileChart/{procedureProfileId}")]
        [Route("procedureProfileChart/{format}/{procedureProfileId}")]
        [HttpPut]
        [HttpPost]
        public async Task<ActionResult> GetProcedureProfileChart(int procedureProfileId, string format = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetProcedureProfileReport(procedureProfileId, user.SelectedLocation);

            var profile = analytics.Tables[0].DefaultView;
            var datasets = new Dictionary<string, DataTable>
            {
                ["ProcedureProfile"] = profile.ToTable()
            };

            var reportName = "ProcedureProfile";

            var result = ReportHelper.GetReport(reportName, format, datasets);

            if (format?.ToUpper() == "PDF")
            {
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return Ok(ResponseHelper.ImageResponse(webImage));
            }
        }

        // GET api/values/5
        [Route("procedureProfileTrayAlignment/{procedureProfileId}")]
        [Route("procedureProfileTrayAlignment/{format}/{procedureProfileId}")]
        [HttpPut]
        [HttpPost]
        public async Task<ActionResult> GetProcedureProfileTrayAlignment([FromBody] ProcedureProfileAlignmentPost post, int procedureProfileId, string format = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetProcedureProfileTrayAlignment(procedureProfileId, post.CardId, post.TrayId, user.SelectedLocation);

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
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                var summary = analytics.Tables[1].DataTableToList<ProcedureProfileAnalyticsSummary>();

                return Ok(ResponseHelper.CompositeImageResponse(summary, webImage));
            }
        }

        // GET api/values/5
        [Route("procedureProfileSupplyAlignment/{procedureProfileId}")]
        [Route("procedureProfileSupplyAlignment/{format}/{procedureProfileId}")]
        [HttpPut]
        [HttpPost]
        public async Task<ActionResult> GetProcedureProfileSupplyAlignment([FromBody] ProcedureProfileAlignmentPost post, int procedureProfileId, string format = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetProcedureProfileSupplyAlignment(procedureProfileId, post.CardId, post.TrayId, user.SelectedLocation);

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
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                var summary = analytics.Tables[1].DataTableToList<ProcedureProfileAnalyticsSummary>();

                return Ok(ResponseHelper.CompositeImageResponse(summary, webImage));
            }
        }

        // GET api/values/5
        [Route("dashboardComparison/{procedureProfileId}")]
        [HttpPost]
        public async Task<ActionResult> PostDashboardComparison(int procedureProfileId, [FromBody]ProcedureProfileDashboardComparisonRequest request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var data = await _sqlHelper.GetProcedureProfileDashboardComparison(procedureProfileId, 
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


            return Ok(new {
                Categories = categories,
                Results = results
            });
        }

        // GET api/values/5
        [Route("trayRequest/{procedureProfileId}")]
        [HttpPut]
        public async Task<ActionResult> PutTrayRequest(int procedureProfileId, [FromBody] TrayRequestEmailRequest request)
        {
            
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            var procedureProfile = await _sqlHelper.GetProcedureProfile(procedureProfileId, null);

            var targets = request.Emails.Select(e => new User() { Email = e });
            var message = await GenerateTrayRequestMessage(procedureProfile);
            var attachments = await GenerateTrayRequestAttachments(procedureProfile);

            await _emailHelper.SendEmail(targets, "Tray Request Summary", message, attachments);

            return Ok(200);
        }

        private async Task<string> GenerateTrayRequestMessage(ProcedureProfile procedureProfile)
        {
            
            var result = string.Empty;

            result += $"{procedureProfile.ProcedureProfileName} Summary\r\n--------\r\n";

            result += "<table><thead><tr><th>Location</th>" +
                "<th>Date</th>" +
                "<th>Surgeon</th>" +
                "<th>Procedure</th>" +
                "<th>CPT</th>" +
                "<th>Questions</th></tr></thead><tbody>";

            var profileSurgeries = await _sqlHelper.GetProcedureProfileSurgeries(procedureProfile.ProcedureProfileID);
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
                var cardItems = await _sqlHelper.GetCardItems(card.CardID, card.LocationID);

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

            var result = new List<EmailHelper.MessageAttachment>();

            foreach (var card in procedureProfile.Cards)
            {
                var cardItems = await _sqlHelper.GetCardItems(card.CardID, card.LocationID);

                var csvData = "Tray, Item, Quantity\r\n";

                foreach (var item in cardItems)
                {
                    if (item.ItemType == "TRAY")
                    {
                        var trayItems = await _sqlHelper.GetTrayItems(item.ItemID, card.LocationID);
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
        [Route("procedureProfileCompare")]
        [HttpPost]
        public async Task<ActionResult> GetProcedureProfileCompare(int procedureProfileId, [FromBody]ProcedureProfileCardComparisonRequest request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var profile = await _sqlHelper.GetProcedureProfile(procedureProfileId, request.LocationFilter);

            var traySummary = profile.TrayItems.GroupBy(p => p.TrayName).Select(p => new ProfileItem()
            {
                ItemType = "TRAY",
                ItemDescription = p.Key,
                Quantity = p.Sum(ti => ti.Quantity)
            });

            var profileItems = profile.Items.Union(profile.TrayItems).Union(traySummary);
            List<CardItem> studyItems = new List<CardItem>();


            var cardItems = await _sqlHelper.GetCardItemsInternal(request.Cards, request.CardCategories);
            studyItems.AddRange(cardItems);
            
            var trayItems = await _sqlHelper.GetTrayItemsInternal(request.Trays);
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

            return Ok(result);
        }

        // GET api/values/5
        [Route("filterCasePreferencesOpp")]
        [HttpPost]
        public async Task<ActionResult> FilterCasePreferencesOpp([FromBody]ProcedureProfileCasePreferenceFilterRequest request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var locationFilter = new List<int> { user.SelectedLocation };

            var profiles = await _sqlHelper.GetProcedureProfiles();
            var proposals = await _sqlHelper.GetProposedTraysInternal(locationFilter);
            var trays = await _sqlHelper.GetTraysInternal(locationFilter);

            if (request.SpecialtyID.HasValue)
            {
                profiles = profiles.Where(p => p.Specialties.Any(s => s.SpecialtyID == request.SpecialtyID.Value)).ToList();
            }

            if ((request.CardCategoryID?.Count() ?? 0) > 0)
                profiles = profiles.Where(p => p.CardCategories.Any(cc => request.CardCategoryID.Contains(cc.CardCategoryID))).ToList();


            return Ok(new
            {
                ProcedureProfiles = profiles,
                Proposals = proposals,
                Trays = trays
            });
        }

        // GET api/values/5
        [Route("executeCasePreferencesOpp")]
        [Route("executeCasePreferencesOpp/{format}")]
        [HttpPut]
        [HttpPost]
        public async Task<ActionResult> ExecuteCasePreferencesOpp([FromBody]ProcedureProfileCasePreferenceReportRequest request, string format = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            if (request.ProcedureProfileID?.Any() != true)
                return Ok(-1);

            var profile = await _sqlHelper.GetProcedureProfileCasePreferencesReport(request.LocationFilter, request.ProcedureProfileID, request.ProposalID, request.TrayID);
            
            if (format == "xlsx")
            {
                var workbook = ExcelHelper.GenerateWorkbook(profile);
                return File(workbook, "application/octet-stream");
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

                await _emailHelper.SendEmail(emailTarget, "Case Preference OPP Report", "Requested Case Preference OPP Report", attachments);
            }

            return Ok(profile);
        }

        [Route("procedureProfileVenn")]
        [HttpPost]
        public async Task<ActionResult> GetProcedureProfileVenn(int procedureProfileId, [FromBody]ProcedureProfileCardComparisonRequest request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            

            var profile = await _sqlHelper.GetProcedureProfile(procedureProfileId, request.LocationFilter);
            var studyItems = await _sqlHelper.GetCardItemsInternal(request.Cards, null);
            var trayItems = await _sqlHelper.GetTrayItemsInternal(request.Trays);

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


            return Ok(new
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
        [Route("category")]
        [HttpGet]
        public async Task<ActionResult> GetProcedureProfileCategories()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.GetProcedureProfileCategories();

            return Ok(result);
        }

        // GET api/values/5
        [Route("category")]
        [HttpPost]
        public async Task<ActionResult> UpdateProcedureProfileCategories(int? categoryId, string categoryName)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.UpdateProcedureProfileCategoryList(categoryId, categoryName);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileUpdate")]
        [HttpPost]
        public async Task<ActionResult> PostProcedureProfileUpdate(int procedureProfileId, [FromBody] ProcedureProfileCategoryPost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.UpdateProcedureProfileCategory(procedureProfileId, request.Name, request.CategoryID, request.OwnerID,
                user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileDashboard")]
        [HttpPost]
        public async Task<ActionResult> PostProcedureProfileDashboard(int procedureProfileId, [FromBody] ProcedureProfileDashboardPost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.UpdateProcedureProfileDashboard(procedureProfileId, request.LocationFilter, request.Cards, request.Trays, request.Proposals,
                user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileStep")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileStep(int procedureProfileId, int stepId, int duration)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.InsertProcedureProfileStep(procedureProfileId, stepId, duration, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileCardCategory")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileCardCategory(int procedureProfileId, int cardCategoryId)
        {
            var user = await GetUserSecurity();

            
            var result = await _sqlHelper.InsertProcedureProfileCardCategory(procedureProfileId, cardCategoryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileStep")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileStep(int procedureProfileId, int stepId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.DeleteProcedureProfileStep(procedureProfileId, stepId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileCardCategory")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileCardCategory(int procedureProfileId, int cardCategoryId)
        {
            var user = await GetUserSecurity();

            

            var result = await _sqlHelper.DeleteProcedureProfileCardCategory(procedureProfileId, cardCategoryId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileStep")]
        [HttpPost]
        public async Task<ActionResult> PostProcedureProfileSteps(int procedureProfileId, [FromBody] ProcedureProfileStepUpdate request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.UpdateProcedureProfileSteps(procedureProfileId, request.Steps, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileCpt")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileCpt(int procedureProfileId, string cptCode)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.InsertProcedureProfileCpt(procedureProfileId, cptCode, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileTray")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileTray(int procedureProfileId, int trayItemId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.Vendor == false)
                return NotFound();
            

            var result = await _sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileTrayGroup")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileTrayGroup(int procedureProfileId, int trayGroupId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.Vendor == false)
                return NotFound();
            

            var trayGroups = await _sqlHelper.GetTrayGroups(user.SelectedLocation);
            var trayGroup = trayGroups.First(t => t.TrayGroupID == trayGroupId);

            await _sqlHelper.InsertProcedureProfileTrayGroup(procedureProfileId, trayGroupId, user.SelectedLocation);

            var result = -1;
            foreach (var tray in trayGroup.Trays)
            {
                result = await _sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, tray.TrayItemID, user.SelectedLocation);
            }

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileCard")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileCard(int procedureProfileId, int cardId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            await _sqlHelper.UpdateProcedureProfileCard(procedureProfileId, cardId, user.SelectedLocation);

            return Ok(cardId);
        }

        // GET api/values/5
        [Route("procedureProfileCardList")]
        [HttpPost]
        public async Task<ActionResult> PostProcedureProfileCardList(int procedureProfileId, [FromBody] InsertCardCategoryCardPost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            await _sqlHelper.UpdateProcedureProfileCardList(procedureProfileId, request.CardId, user.SelectedLocation);

            return Ok(procedureProfileId);
        }

        // GET api/values/5
        [Route("procedureProfileCard")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileCard(int procedureProfileId, int cardId)
        {
            var user = await GetUserSecurity();

            

            var result = await _sqlHelper.DeleteProcedureProfileCard(procedureProfileId, cardId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileItem")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureProfileItem(int procedureProfileId, [FromBody] ProcedureProfileItemUpdatePost post)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            foreach (var item in post.Items)
            {
                if (item.ItemType == "I") // Instrument
                    await _sqlHelper.UpdateProcedureProfileItem(procedureProfileId, null, item.ItemID, item.Category, item.Quantity, user.SelectedLocation);
                else // Supply
                    await _sqlHelper.UpdateProcedureProfileItem(procedureProfileId, item.ItemID, null, item.Category, item.Quantity, user.SelectedLocation);
            }

            return Ok(200);
        }

        // GET api/values/5
        [Route("procedureProfileTrayInstrument")]
        [HttpPost]
        public async Task<ActionResult> PostProcedureProfileTrayInstrument(int procedureProfileId, [FromBody] ProcedureProfileItemUpdatePost post)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            foreach (var item in post.Items)
            {
                await _sqlHelper.UpdateProcedureProfileTrayInstrument(procedureProfileId, item.ItemID, item.TrayItemID, 
                    item.CategoryID, item.Quantity, item.Reason, user.SelectedLocation);
            }

            return Ok(200);
        }

        // GET api/values/5
        [Route("procedureProfileCpt")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileCpt(int procedureProfileId, string cptCode)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            var result = await _sqlHelper.DeleteProcedureProfileCpt(procedureProfileId, cptCode, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("procedureProfileItem")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileItem(int procedureProfileId, string itemType, int itemId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            if (itemType == "I") // Instrument
                await _sqlHelper.DeleteProcedureProfileItem(procedureProfileId, null, itemId, user.SelectedLocation);
            else // Supply
                await _sqlHelper.DeleteProcedureProfileItem(procedureProfileId, itemId, null, user.SelectedLocation);

            return Ok(itemId);
        }

        // GET api/values/5
        [Route("procedureProfileTray")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileTray(int procedureProfileId, int trayId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.Vendor == false)
                return NotFound();
            

            await _sqlHelper.DeleteProcedureProfileTray(procedureProfileId, trayId, user.SelectedLocation);

            return Ok(trayId);
        }

        // GET api/values/5
        [Route("procedureProfileTrayInstrument")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureProfileTrayInstrument(int procedureProfileId, int itemId, int trayItemId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();
            

            await _sqlHelper.DeleteProcedureProfileTrayInstrument(procedureProfileId, itemId, trayItemId, user.SelectedLocation);

            return Ok(itemId);
        }

        // GET api/values/5
        [Route("trayImportCsv")]
        [HttpPut]
        public async Task<ActionResult> PutTrayImportCsv(IFormFile file, int procedureProfileId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal")
                return NotFound();

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            int? logId = null;

            var importTypeId = 3;

            try
            {

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

                var fileParser = new FileParser(fileName, fileContents);

                await fileParser.ParseFile(_sqlHelper, importTypeId, user.SelectedLocation);

                var trayIds = new List<int>();

                foreach (var record in fileParser.Records)
                {
                    var result = await _sqlHelper.InsertStagingData(user.SelectedLocation, null, record, fileParser.Relations, user.UserID);
                    foreach (var message in result.Messages)
                    {
                        await _sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message,
                            null, null);
                    }

                    trayIds.Add(result.Identity);
                }

                foreach (var trayItemId in trayIds.Distinct())
                {
                    await _sqlHelper.InsertProcedureProfileTrayInstrument(procedureProfileId, trayItemId, user.SelectedLocation);
                }

                return Ok(fileParser.Status);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                if (logId != null)
                    await _sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "ERROR", ex.Message, null, null);

                throw;
            }
        }

        // GET api/values/5
        [Route("cptCode")]
        public async Task<ActionResult> GetCptCodes()
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetCptCodes();

            return Ok(result);
        }
    }
}