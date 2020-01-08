using System;
using System.Collections;
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
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/analytics")]
    public class AnalyticsController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("GetReports")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetReports()
        {
            var user = await CacheUtil.GetUserSecurity();

            var sqlHelper = new SqlHelper();
            var reports = await sqlHelper.GetPowerBIReports(user.ProviderID, user.LocationID);
            var specialties = await sqlHelper.GetSpecialties(user.ProviderID, user.LocationID);
            var surgeons = await sqlHelper.GetSurgeons(null, user.ProviderID, user.LocationID);
            var procedures = await sqlHelper.GetProcedures(null, user.ProviderID, user.LocationID);
            var trays = await sqlHelper.GetItems("TRAY", null, null, user.ProviderID, user.LocationID);
            var cardCategories = await sqlHelper.GetCardCategories(user.ProviderID, user.LocationID);
            var lookups = await sqlHelper.GetTrayInstrumentLookups(user.ProviderID, user.LocationID);
            var items = await sqlHelper.GetItems(null, null, true, user.ProviderID, user.LocationID);
            var roomGroups = await sqlHelper.GetRoomGroups(user.ProviderID, user.LocationID);
            var cpts = await sqlHelper.GetKnownCPTCodes(user.ProviderID, user.LocationID);
            var proposedTrays = await sqlHelper.GetProposedTrays(null, user.ProviderID, user.LocationID);
            var proposalPhases = await sqlHelper.GetTrayProposalPhases(user.ProviderID, user.LocationID);
            var caseProfiles = await sqlHelper.GetCaseProfiles(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                ReportCategories = reports,
                Parameters = new
                {
                    Specialties = specialties,
                    Surgeons = surgeons,
                    Trays = trays,   
                    Procedures = procedures,
                    RoomGroups = roomGroups,
                    InstrumentCategories = lookups.Categories,
                    Items = items.Where(i => i.ItemType != "INSTRUMENT"),
                    CardCategories = cardCategories,
                    CPTs = cpts,
                    ProposedTrays = proposedTrays,
                    ProposalPhases = proposalPhases,
                    CaseProfiles = caseProfiles
                }
            });
        }

        // GET api/values/5
        [SwaggerOperation("InstrumentUsageReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("instrumentUsage")]
        [Route("instrumentUsage/{format}")]
        public async Task<HttpResponseMessage> InstrumentUsageReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CategoryID == null &&
                post.ProcedureID == null &&
                (post.Cpt == null || !post.Cpt.Any()) &&
                post.TrayID == null &&
                post.InstrumentID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetInstrumentUsageReportData(post.SpecialtyID, post.SurgeonID, post.CategoryID, post.ProcedureID, post.Cpt, post.TrayID, post.InstrumentID, user.ProviderID, user.LocationID);

            var usage = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "qty":
                    usage.Sort = "QtyOpen DESC";
                    break;
                case "tray":
                    usage.Sort = "TrayName DESC";
                    break;
                case "instrument":
                default:
                    usage.Sort = "Instrument";
                    break;
            }

            if (format == "CSV")
            {
                return ResponseHelper.CsvResponse(usage.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["InstrumentUsage"] = usage.ToTable()
            };

            var reportName = "InstrumentUsage";
            switch (post.Group)
            {
                case "c":
                    reportName = "InstrumentUsageCard";
                    break;
                case "t_c":
                    reportName = "InstrumentUsageTrayCard";
                    break;
                case "c_t":
                    reportName = "InstrumentUsageCardTray";
                    break;
                case "s_c":
                    reportName = "InstrumentUsageSurgeonCard";
                    break;
            }

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
        [SwaggerOperation("DisposableUsageReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("disposableUsage")]
        [Route("disposableUsage/{format}")]
        public async Task<HttpResponseMessage> DisposableUsageReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.CardCategoryID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetDisposableUsageReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.CardCategoryID, 
                user.ProviderID, user.LocationID);

            var usage = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "qty":
                    usage.Sort = "QtyOpen DESC";
                    break;
                case "tray":
                    usage.Sort = "TrayName DESC";
                    break;
                case "item":
                default:
                    usage.Sort = "ItemName";
                    break;
            }

            if (format == "CSV")
            {
                return ResponseHelper.CsvResponse(usage.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["DisposableUsage"] = usage.ToTable()
            };

            var reportName = "DisposableUsage";

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
        [SwaggerOperation("TrayConcordanceReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("trayConcordanceReport")]
        [Route("trayConcordanceReport/{format}")]
        public async Task<HttpResponseMessage> TrayConcordanceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.CardCategoryID == null &&
                post.CardID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error= true});
            }

            var analytics = await sqlHelper.GetConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, post.TrayID, 
                post.CardCategoryID, post.CardID, post.Instruments, user.ProviderID, user.LocationID);

            var summary = SummarizeConcordanceReport(analytics.Tables[0]);

            var concordance = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "instrument_avg":
                    concordance.Sort = "QtyOpen DESC";
                    break;
                case "instrument_name":
                default:
                    concordance.Sort = "InstrumentName";
                    break;
            }

            if (format == "CSV")
            {
                return ResponseHelper.CsvResponse(concordance.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ConcordanceReport"] = concordance.ToTable()
            };
            var result = ReportHelper.GetReport("ConcordanceReport", format, datasets);
            
            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
            }
        }

        // GET api/values/5
        [SwaggerOperation("SupplyConcordanceReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("supplyConcordanceReport")]
        [Route("supplyConcordanceReport/{format}")]
        public async Task<HttpResponseMessage> SupplyConcordanceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.CardCategoryID == null &&
                post.CardID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetSupplyConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, 
                post.CardCategoryID, post.CardID, post.Instruments, user.ProviderID, user.LocationID);

            var summary = SummarizeConcordanceReport(analytics.Tables[0]);

            var concordance = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "usage":
                    concordance.Sort = "QtyOpen DESC, TrayUsage DESC";
                    summary = summary.OrderByDescending(s => s.ItemUsage).ToList();
                    break;
                case "card_qty":
                default:
                    concordance.Sort = "TrayQty DESC, InstrumentName";
                    summary = summary.OrderByDescending(s => s.ItemQuantity).ToList();
                    break;
            }


            if (format == "CSV")
            {
                return ResponseHelper.CsvResponse(concordance.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ConcordanceReport"] = concordance.ToTable()
            };
            var result = ReportHelper.GetReport("ConcordanceReport", format, datasets);

            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
            }
        }

        private List<ConcordanceReportSummary> SummarizeConcordanceReport(DataTable concordanceData)
        {
            var result = new List<ConcordanceReportSummary>();

            foreach (DataRow concordanceRow in concordanceData.Rows)
            {
                var trayQty = concordanceRow["TrayQty"];
                var tray = concordanceRow["SurgeonName"].ToString();

                if (trayQty != DBNull.Value)
                {
                    var quantity = (decimal)trayQty;
                    var usageQty = (decimal)concordanceRow["TrayUsage"];

                    var usage = result.FirstOrDefault(r => r.TrayName == tray);
                    if (usage == null)
                    {
                        usage = new ConcordanceReportSummary()
                        {
                            TrayName = tray
                        };
                        result.Add(usage);
                    }

                    usage.TrayItems.Add(new ConcordanceItem()
                    {
                        Usage = usageQty,
                        Quantity = quantity
                    });
                }
            }

            return result;
        }

        // GET api/values/5
        [SwaggerOperation("VendorTrayConcordanceReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("vendorTrayConcordanceReport")]
        [Route("vendorTrayConcordanceReport/{format}")]
        public async Task<HttpResponseMessage> VendorTrayConcordanceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.CardCategoryID == null &&
                post.CardID == null &&
                post.CaseProfileId == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetVendorTrayConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, post.TrayID,
                post.CardCategoryID, post.CardID, post.Instruments, post.CaseProfileId, post.QuestionId, post.AnswerId, user.ProviderID, user.LocationID,
                post.Group);

            var concordance = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "instrument_avg":
                    concordance.Sort = "QtyOpen DESC";
                    break;
                case "instrument_name":
                default:
                    concordance.Sort = "InstrumentName";
                    break;
            }

            var summary = SummarizeConcordanceReport(analytics.Tables[0]);

            if (format == "CSV")
            {
                return ResponseHelper.CsvResponse(concordance.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ConcordanceReport"] = concordance.ToTable()
            };
            var result = ReportHelper.GetReport("ConcordanceReport", format, datasets);

            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
            }
        }

        // GET api/values/5
        [SwaggerOperation("SupplyWasteReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("supplyWaste")]
        [Route("supplyWaste/{format}")]
        public async Task<HttpResponseMessage> SupplyWasteReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetSupplyWasteReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID, 
                post.MinCost, post.MinOpen, post.MinHold, post.StartDate, post.EndDate, true, true, true, true, post.Group, user.ProviderID, user.LocationID);

            var supplyWaste = analytics.Tables[0].DefaultView;
            
            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyWaste"] = supplyWaste.ToTable()
            };
            var result = ReportHelper.GetReport("SupplyWaste", format, datasets);

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
        [SwaggerOperation("ServiceLineReviewReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("serviceLineReview")]
        [Route("serviceLineReview/{format}")]
        public async Task<HttpResponseMessage> ServiceLineReviewReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.StartDate == null &&
                post.EndDate == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            if (format?.ToUpper() == "CSV")
            {
                // override some of the parameters to force more detail
                post.Group = "SCI";
            }

            var analytics = await sqlHelper.GetServiceLineReviewReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID,
                post.MinCost, post.MinOpen, post.MinHold, 
                post.FieldAll, post.FieldWaste, post.FieldOver, post.FieldUnder,
                post.StartDate, post.EndDate, post.Group, user.ProviderID, user.LocationID);

            var supplyOpen = analytics.Tables[0].DefaultView;
            var supplyOpenAggregate = analytics.Tables[1].DefaultView;

            if (format?.ToUpper() == "CSV")
            {
                return ResponseHelper.CsvResponse(supplyOpen.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ServiceLineReview"] = supplyOpenAggregate.ToTable()
            };
            var result = ReportHelper.GetReport("ServiceLineReview", format, datasets);

            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var summary = analytics.Tables[1].DataTableToList<ServiceLineSummary>();

                var summarySpecialties = summary.GroupBy(s => s.Specialty).Select(x => new ServiceLineSummarySpecialty()
                {
                    Specialty = x.Key,
                    Results = x.ToList()
                });

                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.CompositeImageResponse(Request, summarySpecialties, webImage);
            }
        }

        // GET api/values/5
        [SwaggerOperation("SupplyOpenReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("supplyOpen")]
        [Route("supplyOpen/{format}")]
        public async Task<HttpResponseMessage> SupplyOpenReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();

            if (post.StartDate == null &&
                post.EndDate == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            if (format?.ToUpper() == "CSV")
            {
                // override some of the parameters to force more detail
                post.Group = "SCI";
            }

            var analytics = await sqlHelper.GetSupplyWasteReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID,
                post.MinCost, post.MinOpen, post.MinHold, post.StartDate, post.EndDate, 
                post.FieldAll, post.FieldWaste, post.FieldOver, post.FieldUnder,
                post.Group, user.ProviderID, user.LocationID);

            var supplyOpen = analytics.Tables[0].DefaultView;
            var supplyOpenAggregate = analytics.Tables[2].DefaultView;
            
            if (format?.ToUpper() == "CSV")
            {
                var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);
                var userObject = await sqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

                var dtSupply = supplyOpen.ToTable();
                dtSupply.Columns.Add("MRN", typeof(string));

                var extract = "MRN,SurgeryDate,SurgeonName,ItemName,Specialty,ProcedureName,ItemCost,CardName,CardQuantity<TotalUsed,SetupOpen,SetupAdded,QtyOpenInit,QtyOpenAdded,OpenVariance,UsageVariance,CardQtyVariance,TotalOpen";
                
                foreach (DataRow dtSupplyData in dtSupply.Rows)
                {
                    extract += "\r\n";

                    var patientId = (int)dtSupplyData["PatientID"];
                    var patient = await secureSqlHelper.GetPatient(patientId, user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

                    var surgeryDate = ((DateTime)dtSupplyData["SurgeryDate"]).ToShortDateString();
                    var surgeon = dtSupplyData["SurgeonName"].ToString().Trim().Replace("\"", "\"\"");
                    var item = dtSupplyData["ItemName"].ToString().Trim().Replace("\"", "\"\"");
                    var specialty = dtSupplyData["Specialty"].ToString().Trim().Replace("\"", "\"\"");
                    var procName = dtSupplyData["ProcedureName"].ToString().Trim().Replace("\"", "\"\"");
                    var cardName = dtSupplyData["CardName"].ToString().Trim().Replace("\"", "\"\"");

                    extract += $"=\"{patient.PatientAcctNbr}\",{surgeryDate},\"{surgeon}\",\"{item}\",\"{specialty}\",\"{procName}\"," +
                        $"{dtSupplyData["ItemCost"]},\"{cardName}\",{dtSupplyData["CardQuantity"]},{dtSupplyData["TotalUsed"]},{dtSupplyData["SetupOpen"]},{dtSupplyData["SetupAdded"]}" +
                        $"{dtSupplyData["QtyOpenInit"]},{dtSupplyData["QtyOpenAdded"]},{dtSupplyData["OpenVariance"]},{dtSupplyData["UsageVariance"]},{dtSupplyData["CardQtyVariance"]},{dtSupplyData["TotalOpen"]}";
                }

                return ResponseHelper.CsvResponse(extract);
            }

            // Data set repeats three times for nested columns - clean things up now
            foreach (DataRow aggregate in supplyOpenAggregate.Table.Rows)
            {
                if (aggregate["UsageType"].ToString() == "1Setup")
                {
                    aggregate["AvgSetup"] = aggregate["AvgSetupOpen"];
                    aggregate["AvgOpen"] = 0;
                }
                if (aggregate["UsageType"].ToString() == "3Added")
                {
                    aggregate["AvgSetup"] = aggregate["AvgSetupAdded"];
                    aggregate["AvgOpen"] = aggregate["AvgOpenAdded"];
                }
                if (aggregate["UsageType"].ToString() == "2Usage")
                {
                    aggregate["AvgSetup"] = 0;
                    aggregate["AvgOpen"] = aggregate["AvgOpenInit"];
                }


                if (aggregate["UsageType"].ToString() != "1Setup")
                {
                    aggregate["UsageVariance"] = 0;
                    aggregate["CardQtyVariance"] = 0;
                    aggregate["AvgUsageVariance"] = 0;
                    aggregate["AvgCardQtyVariance"] = 0;
                }
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyOpen"] = supplyOpenAggregate.ToTable()
            };
            var result = ReportHelper.GetReport("SupplyOpen", format, datasets);

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
        [SwaggerOperation("SupplyUsageVarianceReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("supplyUsageVariance")]
        [Route("supplyUsageVariance/{format}")]
        public async Task<HttpResponseMessage> SupplyUsageVarianceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetSupplyWasteReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID,
                post.MinCost, post.MinOpen, post.MinHold, post.StartDate, post.EndDate, true, true, true, true, post.Group, user.ProviderID, user.LocationID);

            var supplyOpen = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyUsageVariance"] = supplyOpen.ToTable()
            };
            var result = ReportHelper.GetReport("SupplyUsageVariance", format, datasets);

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
        [SwaggerOperation("CardRedundancyReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("cardRedundancy")]
        [Route("cardRedundancy/{format}")]
        public async Task<HttpResponseMessage> CardRedundancyReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetCardRedundancyReport(post.SpecialtyID, post.SurgeonID, post.CardID, post.MinQty, post.Redundancy, user.ProviderID, user.LocationID);

            var cardRedundancy = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["CardRedundancy"] = cardRedundancy.ToTable()
            };
            var result = ReportHelper.GetReport("CardRedundancy", format, datasets);

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
        [SwaggerOperation("ExcessInventoryReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("excessInventory")]
        [Route("excessInventory/{format}")]
        public async Task<HttpResponseMessage> ExcessInventoryReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.ProposedTrayID == null && post.SpecialtyID == null && post.TrayStatus == null && post.TrayPhaseID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetExcessInventoryReport(post.SpecialtyID, post.ProposedTrayID, post.TrayStatus, post.TrayPhaseID,                 
                post.Group, user.ProviderID, user.LocationID);

            var excessInventory = analytics.Tables[0].DefaultView;

            if (format == "CSV")
            {
                return ResponseHelper.CsvResponse(excessInventory.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ExcessInventory"] = excessInventory.ToTable()
            };
            var result = ReportHelper.GetReport("ExcessInventory", format, datasets);

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
        [SwaggerOperation("SupplyCostReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("supplyCost")]
        [Route("supplyCost/{format}")]
        public async Task<HttpResponseMessage> SupplyCostReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.StartDate == null &&
                post.EndDate == null &&
                post.CardCategoryID == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetSupplyCardCostReportData(post.SpecialtyID, post.SurgeonID, post.CardCategoryID, 
                post.StartDate, post.EndDate, user.ProviderID, user.LocationID);

            var supplyWaste = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyCost"] = supplyWaste.ToTable()
            };
            var result = ReportHelper.GetReport("SupplyCost", format, datasets);

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
        [SwaggerOperation("SupplySavingsEstimatorReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<InstrumentUsageSummaryResult>))]
        [HttpPut]
        [HttpPost]
        [Route("supplySavings")]
        [Route("supplySavings/{format}")]
        public async Task<HttpResponseMessage> SupplySavingsEstimatorReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.CardCategoryID == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ItemID == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetSupplySavingsEstimatorReportData(post.SpecialtyID, post.SurgeonID, post.CardCategoryID,
                post.ItemID, user.ProviderID, user.LocationID);

            var supplySavings = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplySavings"] = supplySavings.ToTable()
            };
            var result = ReportHelper.GetReport("SupplySavingsEstimator", format, datasets);

            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);
                var summary = SummarizeSupplySavings(supplySavings.ToTable());

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
            }
        }

        private SupplySavingsSummary SummarizeSupplySavings(DataTable supplySavings)
        {
            var result = new SupplySavingsSummary();

            foreach(DataRow drSaving in supplySavings.Rows)
            {
                var totalOpen = (decimal)drSaving["QtyOpen"];
                var waste = (decimal)drSaving["QtySetup"] - totalOpen;
                var overallocation = (int)drSaving["CardQty"] - totalOpen;
                var cost = (decimal)drSaving["UnitCost"];

                if (waste < 0)
                    waste = 0;
                if (overallocation < 0)
                    overallocation = 0;

                result.TotalCost += totalOpen * cost;
                result.WasteUnits += waste;
                result.OverallocatedUnits += overallocation;
                result.WasteCost += waste * cost;
                result.OverallocatedCost += overallocation * cost;
            }

            return result;
        }

        // GET api/values/5
        [SwaggerOperation("TrayConsolidationReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("trayConsolidation")]
        [Route("trayConsolidation/{format}")]
        public async Task<HttpResponseMessage> TrayConsolidationReport([FromBody] TrayConsolidationReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();


            var analytics = await sqlHelper.GetAnalyticsTrayConsolidationData(post.SpecialtyId, post.TrayId, post.Reallocation,
                post.MaxSize, post.MinCards, post.MinConsolidationInstances, post.MinTargetInstances, post.Overlap, post.Effect, post.ProcedureGroup, post.Group, user.ProviderID, user.LocationID);
            
            return Request.CreateResponse(HttpStatusCode.OK, analytics);
            /*
            var consolidation = new DataView(analytics);
            consolidation.Sort = "CardCount DESC";

            if (format?.ToUpper() == "CSV")
            {
                return ResponseHelper.CsvResponse(consolidation.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayConsolidation"] = consolidation.ToTable()
            };

            var reportParams = new[] {
                new ReportParameter("Validation", (post.Reallocation.HasValue ? "true" : "false"))
            };
            var result = ReportHelper.GetReport("TrayConsolidation", format, datasets, reportParams);

            if (format?.ToUpper() == "PDF")
            {
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.ImageResponse(Request, webImage);
            }*/
        }

        // GET api/values/5
        [SwaggerOperation("TrayConsolidationDownload")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("trayConsolidationDownload")]
        public async Task<HttpResponseMessage> TrayConsolidationDownload([FromBody] TrayConsolidationExportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();


            var export = "Tray 1, Tray 2, Shared Card\r\n";

            foreach (var exportTray in post.Exports)
            {
                var cards = await sqlHelper.GetTrayCards(exportTray.TrayID, user.ProviderID, user.LocationID);

                if (exportTray.Children == null || !exportTray.Children.Any())
                {
                    foreach (var card in cards)
                    {
                        export += $"{card.TrayName},NULL,{card.CardDescription}\r\n";
                    }
                }

                foreach (var child in exportTray?.Children ?? new List<TrayConsolidationExport>())
                {
                    var cards2 = await sqlHelper.GetTrayCards(child.TrayID, user.ProviderID, user.LocationID);

                    foreach (var card2 in cards2.Where(c2 => cards.Any(c => c2.CardID == c.CardID)))
                    {
                        var card1 = cards.FirstOrDefault(c => c.CardID == card2.CardID);
                        export += $"{card1?.TrayName},{card2.TrayName},{card2.CardDescription}\r\n";
                    }
                    
                    foreach (var child2 in child?.Children ?? new List<TrayConsolidationExport>())
                    {
                        var cards3 = await sqlHelper.GetTrayCards(child2.TrayID, user.ProviderID, user.LocationID);

                        foreach (var card3 in cards3.Where(c3 => cards2.Any(c2 => c3.CardID == c2.CardID)))
                        {
                            var card2 = cards2.FirstOrDefault(c => c.CardID == card3.CardID);
                            export += $"{card2.TrayName},{card3.TrayName},{card3.CardDescription}\r\n";
                        }
                    }
                }
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(export);
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

        // GET api/values/5
        [SwaggerOperation("TrayRationalizationReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("trayRationalization")]
        [Route("trayRationalization/{format}")]
        public async Task<HttpResponseMessage> TrayRationalizationReport([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, post.CardCategoryId,
                post.MinSize, user.ProviderID, user.LocationID);

            var rationalization = new DataView(analytics.Tables[0]);
            
            switch (post.Order)
            {
                case "instrument_nbr":
                    rationalization.Sort = "InstrumentCount";
                    break;
                case "instrument_avg":
                    rationalization.Sort = "UsageQuantity DESC";
                    break;
                case "tray_open":
                    rationalization.Sort = "TrayOpened";
                    break;
                case "tray_name":
                    rationalization.Sort = "TrayName DESC";
                    break;
                case "count":
                default:
                    rationalization.Sort = "CaseCount";
                    break;
            }

            var parameters = new ReportParameter[]
            {
                new ReportParameter("FieldCase", post.FieldCase.ToString()),
                new ReportParameter("FieldInstrument", post.FieldInstrument.ToString()),
                new ReportParameter("FieldUsage", post.FieldUsage.ToString()),
                new ReportParameter("FieldTray", post.FieldTray.ToString())
            };

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayRationalization"] = rationalization.ToTable()
            };
            var result = ReportHelper.GetReport("TrayRationalization", format, datasets, parameters);
            
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
        [SwaggerOperation("VendorTrayRationalizationReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("vendorTrayRationalization")]
        [Route("vendorTrayRationalization/{format}")]
        public async Task<HttpResponseMessage> VendorTrayRationalizationReport([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAnalyticsVendorTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, post.CardCategoryId,
                post.MinSize, post.StartDate, post.EndDate, post.CaseProfileId, post.QuestionId, post.AnswerId, user.ProviderID, user.LocationID);

            var rationalization = new DataView(analytics.Tables[0]);

            switch (post.Order)
            {
                case "instrument_nbr":
                    rationalization.Sort = "InstrumentCount";
                    break;
                case "instrument_avg":
                    rationalization.Sort = "UsageQuantity DESC";
                    break;
                case "tray_open":
                    rationalization.Sort = "TrayOpened";
                    break;
                case "tray_name":
                    rationalization.Sort = "TrayName DESC";
                    break;
                case "count":
                default:
                    rationalization.Sort = "CaseCount";
                    break;
            }


            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayRationalization"] = rationalization.ToTable()
            };

            var parameters = new ReportParameter[]
            {
                new ReportParameter("FieldCase", post.FieldCase.ToString()),
                new ReportParameter("FieldInstrument", post.FieldInstrument.ToString()),
                new ReportParameter("FieldUsage", post.FieldUsage.ToString()),
                new ReportParameter("FieldTray", post.FieldTray.ToString())
            };

            var result = ReportHelper.GetReport("TrayRationalization", format, datasets, parameters);

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
        [SwaggerOperation("TrayScopeReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("trayScope")]
        [Route("trayScope/{format}")]
        public async Task<HttpResponseMessage> TrayScopeReport([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAnalyticsTrayScopeData(post.SpecialtyId, post.TrayId, user.ProviderID, user.LocationID);

            var rationalization = new DataView(analytics.Tables[0]);

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayScope"] = rationalization.ToTable()
            };
            var result = ReportHelper.GetReport("TrayScope", format, datasets);

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
        [SwaggerOperation("TrayRationalizationReportImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("trayRationalizationImage")]
        [Route("trayRationalizationImage/{format}")]
        public async Task<HttpResponseMessage> TrayRationalizationReportImage([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, post.CardCategoryId, post.MinSize, user.ProviderID, user.LocationID);
            /*
            switch (post.Order)
            {
                case "instrument_nbr":
                    analytics = analytics.OrderByDescending(a => a.InstrumentCount).ToList();
                    break;
                case "instrument_avg":
                    analytics = analytics.OrderByDescending(a => a.UsageQuantity).ToList();
                    break;
                case "tray_open":
                    analytics = analytics.OrderByDescending(a => a.TrayOpened).ToList();
                    break;
                case "tray_name":
                    analytics = analytics.OrderBy(a => a.TrayName).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.CaseCount).ToList();
                    break;
            }*/

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayRationalization"] = analytics.Tables[0]
            };
            var result = ReportHelper.GetReport("TrayRationalization", format, datasets);

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
        [SwaggerOperation("CountSummaryReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("countSummary")]
        [Route("countSummary/{format}")]
        public async Task<HttpResponseMessage> CountSummaryReport([FromBody] CountSummaryReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, post.SurgeonId, post.CardId, post.CardCategoryId,
                post.RoomGroupId, user.ProviderID, user.LocationID);

            /*switch (post.Order)
            {
                case "specialty":
                    analytics = analytics.OrderBy(a => a.Specialty).ToList();
                    break;
                case "card":
                    analytics = analytics.OrderBy(a => a.Card).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.TrayCount).ToList();
                    break;
            }*/

            var parameters = new[]
            {
                new ReportParameter("Group", "t")
            };
            var datasets = new Dictionary<string, DataTable>
            {
                ["CountSummary"] = analytics.Tables[0]
            };
            var result = ReportHelper.GetReport("CountSummary", format, datasets, parameters);

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
        [SwaggerOperation("CountSummaryByCardReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("countSummaryByCard")]
        [Route("countSummaryByCard/{format}")]
        public async Task<HttpResponseMessage> CountSummaryByCardReport([FromBody] CountSummaryReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            format = format ?? "IMAGE";
            
            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, post.SurgeonId, post.CardId, post.CardCategoryId,
                post.RoomGroupId, user.ProviderID, user.LocationID);

            /*            switch (post.Order)
            {
                case "specialty":
                    analytics = analytics.OrderBy(a => a.Specialty).ToList();
                    break;
                case "card":
                    analytics = analytics.OrderBy(a => a.Card).ToList();
                    break;
                case "count":
                default:
                    analytics = analytics.OrderByDescending(a => a.TrayCount).ToList();
                    break;
            }*/

            var parameters = new[]
            {
                new ReportParameter("Group", "c")
            };
            var datasets = new Dictionary<string, DataTable>
            {
                ["CountSummary"] = analytics.Tables[0]
            };
            var result = ReportHelper.GetReport("CountSummary", format, datasets, parameters);

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
    }
}
