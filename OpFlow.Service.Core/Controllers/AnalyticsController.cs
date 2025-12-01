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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Reporting.NETCore;
using OpFlow.Data;
using OpFlow.Service.Controllers;
using OpFlow.Service.DataAccess;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/analytics")]
    public class AnalyticsController : OpFlowController
    {
        public AnalyticsController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult> GetReports()
        {
            var user = await GetUserSecurity();

            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var surgeons = await _sqlHelper.GetSurgeons(null, user.SelectedLocation);
            var procedures = await _sqlHelper.GetProcedures(null, user.SelectedLocation);
            var trays = await _sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation);
            var cardCategories = await _sqlHelper.GetCardCategories();
            var lookups = await _sqlHelper.GetTrayInstrumentLookups(user.SelectedLocation);
            var items = await _sqlHelper.GetItems(null, null, true, user.SelectedLocation);
            var roomGroups = await _sqlHelper.GetRoomGroups(user.SelectedLocation);
            var cpts = await _sqlHelper.GetKnownCPTCodes(user.ProviderID, user.LocationID);
            var proposedTrays = await _sqlHelper.GetProposedTrays(null, user.SelectedLocation);
            var proposalPhases = await _sqlHelper.GetTrayProposalPhases(user.SelectedLocation);
            var caseProfiles = await _sqlHelper.GetCaseProfiles(user.SelectedLocation);

            var procedureProfiles = await _sqlHelper.GetProcedureProfiles();
            var trayTypes = await _sqlHelper.GetTrayTypes();
            var metrics = await _sqlHelper.GetProcedureProfileMetrics(null, user.SelectedLocation);

            var cardCategoryXref = await _sqlHelper.GetSpecialtyProcedureGroup(user.SelectedLocation);

            foreach (var specialty in specialties)
            {
                specialty.CardCategories = cardCategoryXref.Where(c => c.SpecialtyID == specialty.SpecialtyID).ToList();
            }

            return Ok(new
            {
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
                    CaseProfiles = caseProfiles,
                    ProcedureProfiles = procedureProfiles,
                    TrayTypes = trayTypes,
                    Metrics = metrics
                }
            });
        }

        private async Task<string> GetLocationName(UserSecurity user)
        {
            var providers = await _sqlHelper.GetOpFlowSetup();

            var provider = providers.FirstOrDefault(p => p.ProviderID == user.ProviderID);
            var location = provider?.Locations?.FirstOrDefault(l => l.LocationID == user.LocationID);

            return location.LocationName ?? "Unknown Location";
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("instrumentUsage")]
        [Route("instrumentUsage/{format}")]
        public async Task<ActionResult> InstrumentUsageReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CategoryID == null &&
                post.ProcedureID == null &&
                (post.Cpt == null || !post.Cpt.Any()) &&
                post.TrayID == null &&
                post.InstrumentID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetInstrumentUsageReportData(post.SpecialtyID, post.SurgeonID, 
                post.CategoryID, post.ProcedureID, post.Cpt, post.TrayID, post.InstrumentID, post.Group, user.SelectedLocation);

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
                    usage.Sort = "Instrument DESC";
                    break;
            }

            if (format == "CSV")
            {
                return CsvResponse(usage.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["InstrumentUsage"] = usage.ToTable()
            };

            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var result = ReportHelper.GetReport($"InstrumentUsage{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("instrumentUsageDistribution")]
        [Route("instrumentUsageDistribution/{format}")]
        public async Task<ActionResult> InstrumentUsageDistributionReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.TrayID == null &&
                post.InstrumentID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetInstrumentUsageDistributionReportData(post.SpecialtyID, post.SurgeonID,
                post.CategoryID, post.ProcedureID, post.Cpt, post.TrayID, post.InstrumentID, post.Group, user.SelectedLocation);

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
                    usage.Sort = "Instrument DESC";
                    break;
            }

            if (format == "CSV")
            {
                return CsvResponse(usage.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["InstrumentUsage"] = usage.ToTable()
            };

            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var result = ReportHelper.GetReport($"InstrumentUsageDistribution{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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

        [Route("traySummary/{trayProposalId}", Name = "GetTraySummary")]
        [HttpGet]
        public async Task<ActionResult> GetTraySummary(int trayProposalId, int timezone)
        {
            var user = await GetUserSecurity();

            var proposedTray = (await _sqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).FirstOrDefault();
            var instruments = await _sqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
            var audits = await _sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.SelectedLocation);
            var trayCounts = await _sqlHelper.GetTrayCountSummary(trayProposalId, user.SelectedLocation);
            var sourceTrays = await _sqlHelper.GetSourceTraySummary(trayProposalId, user.SelectedLocation);
            var cardOverlaps = await _sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.SelectedLocation);

            // Error path
            if (proposedTray == null) proposedTray = new TrayRationalization();

            proposedTray.InstrumentCount = instruments.Sum(i => i.Quantity);
            foreach (var sourceTray in sourceTrays)
            {
                sourceTray.InstrumentCount = sourceTray.Instruments.Sum(i => i.Quantity);
                sourceTray.ProposedInstrumentCount = instruments.Sum(i => i.Quantity);
            }

            var parameters = new[]
            {
                new ReportParameter("Target", await GetLocationName(user)),
                new ReportParameter("Timezone", timezone.ToString())
            };

            var datasets = new Dictionary<string, DataTable>
            {
                ["ProposedTray"] = (new List<TrayRationalization>() { proposedTray }).ToDataTable(),
                ["TrayInstruments"] = instruments.ToDataTable(),
                ["Audits"] = audits.Where(a => a.AuditUserID.HasValue).OrderBy(a => a.SurgeonName).ToList().ToDataTable(),
                ["TrayCounts"] = trayCounts.ToDataTable(),
                ["SourceTrays"] = sourceTrays.ToDataTable(),
                ["Instruments"] = instruments.ToDataTable(),
                ["Cards"] = cardOverlaps.ToDataTable()
            };

            var result = ReportHelper.GetReport($"TrayApproval", "PDF", datasets, parameters);

            return File(result, "application/octet-stream");
        }

        [Route("trayAnalyticSummary/{trayProposalId}", Name = "GetTrayAnalyticSummary")]
        [HttpPut]
        public async Task<ActionResult> GetTrayAnalyticSummary(int trayProposalId, [FromBody] TrayRationalizationReportPost post)
        {
            var user = await GetUserSecurity();

            var proposedTray = (await _sqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation)).FirstOrDefault();
            var instruments = await _sqlHelper.GetProposedTrayInstruments(trayProposalId, user.SelectedLocation);
            var audits = await _sqlHelper.GetProposedTrayAudits(trayProposalId, null, null, user.SelectedLocation);
            var trayCounts = await _sqlHelper.GetTrayCountSummary(trayProposalId, user.SelectedLocation);
            var sourceTrays = await _sqlHelper.GetSourceTraySummary(trayProposalId, user.SelectedLocation);
            var cardOverlaps = await _sqlHelper.GetProposedTrayCardOverlap(trayProposalId, user.SelectedLocation);

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

            var countAnalytics = await _sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, 
                null, null, null, null, null, null, "tray", user.SelectedLocation);
            var instrumentAnalytics = await _sqlHelper.GetInstrumentUsageReportData(post.SpecialtyId, 
                null, null, null, null, post.TrayId, null, "t", user.SelectedLocation);
            var trayAnalytics = await _sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, 
                null, post.TrayId, post.TrayTypeId, post.VendorTray, null, null, null, null, user.SelectedLocation);

            var baseParameters = new[]
            {
                new ReportParameter("Target", await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var groupParameters = new[]
            {
                new ReportParameter("Group", "t"),
                new ReportParameter("Target", await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var countDatasets = new Dictionary<string, DataTable>
            {
                ["CountSummary"] = countAnalytics.Tables[0]
            };
            var countSummaryBytes = ReportHelper.GetReport("CountSummaryExport", "PDF", countDatasets, groupParameters);
            var instrumentDatasets = new Dictionary<string, DataTable>
            {
                ["InstrumentUsage"] = instrumentAnalytics.Tables[0]
            };
            var instrumentUsageBytes = ReportHelper.GetReport("InstrumentUsageExport", "PDF", instrumentDatasets, baseParameters);
            var trayDatasets = new Dictionary<string, DataTable>
            {
                ["TrayRationalization"] = trayAnalytics.Tables[0]
            };
            var trayRationalizationBytes = ReportHelper.GetReport("TrayRationalizationExport", "PDF", trayDatasets, baseParameters);

            //    countSummaryBytes,
            //    instrumentUsageBytes,
            //    trayRationalizationBytes
            var countStream = new MemoryStream(countSummaryBytes);
            var usageStream = new MemoryStream(instrumentUsageBytes);
            var rationalizationStream = new MemoryStream(trayRationalizationBytes);

            var datasets = new Dictionary<string, DataTable>
            {
                ["ProposedTray"] = (new List<TrayRationalization>() { proposedTray }).ToDataTable(),
                ["TrayInstruments"] = instruments.ToDataTable(),
                ["Audits"] = audits.Where(a => a.AuditUserID.HasValue).OrderBy(a => a.SurgeonName).ToList().ToDataTable(),
                ["TrayCounts"] = trayCounts.ToDataTable(),
                ["SourceTrays"] = sourceTrays.ToDataTable(),
                ["Instruments"] = instruments.ToDataTable(),
                ["Cards"] = cardOverlaps.ToDataTable()
            };

            var result = ReportHelper.GetReport($"TrayApproval", "PDF", datasets, baseParameters);
            var summaryStream = new MemoryStream(result);

            var resultStream = new MemoryStream();

            using (var summaryPdf = PdfReader.Open(summaryStream, PdfDocumentOpenMode.Import))
            using (var countsPdf = PdfReader.Open(countStream, PdfDocumentOpenMode.Import))
            using (var usagePdf = PdfReader.Open(usageStream, PdfDocumentOpenMode.Import))
            using (var rationalizationPdf = PdfReader.Open(rationalizationStream, PdfDocumentOpenMode.Import))
            using (var responsePdf = new PdfDocument())
            {
                CopyPages(countsPdf, responsePdf);
                CopyPages(usagePdf, responsePdf);
                CopyPages(rationalizationPdf, responsePdf);

                CopyPages(summaryPdf, responsePdf);

                responsePdf.Save(resultStream);
            }

            var resultBytes = resultStream.GetBuffer();
            return File(resultBytes, "application/octet-stream");
        }

        private void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("disposableUsage")]
        [Route("disposableUsage/{format}")]
        public async Task<ActionResult> DisposableUsageReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.CardCategoryID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetDisposableUsageReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.CardCategoryID, 
                user.SelectedLocation);

            var usage = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "qty":
                    usage.Sort = "QtyOpen DESC";
                    break;
                case "case":
                    usage.Sort = "ItemCounts DESC";
                    break;
                case "item":
                default:
                    usage.Sort = "ItemName";
                    break;
            }

            if (format == "CSV")
            {
                return CsvResponse(usage.ToTable());
            }
            
            var parameters = new ReportParameter[]
            {
                new ReportParameter("Order", post.Order),
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var datasets = new Dictionary<string, DataTable>
            {
                ["DisposableUsage"] = usage.ToTable()
            };

            var result = ReportHelper.GetReport($"DisposableUsage{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("trayConcordanceReport")]
        [Route("trayConcordanceReport/{format}")]
        public async Task<ActionResult> TrayConcordanceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.CardCategoryID == null &&
                post.CardID == null)
            {
                return Ok(new { Error= true});
            }

            var analytics = await _sqlHelper.GetConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, post.TrayID, post.TrayTypeID, post.VendorTray,
                post.CardCategoryID, post.CardID, post.MetricID, post.RoomGroupID, post.Instruments, post.ShowMax, post.Label, user.SelectedLocation);

            var summary = SummarizeConcordanceReport(analytics.Tables[0], analytics.Tables[1]);
            
            var concordance = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "instrument_avg":
                    concordance.Sort = "QtyOpen DESC";
                    summary.Items = summary.Items.OrderByDescending(i => i.QtyOpen).ToList();
                    break;
                case "instrument_name":
                default:
                    concordance.Sort = "InstrumentName";
                    summary.Items = summary.Items.OrderBy(i => i.InstrumentDescription).ToList();
                    break;
            }

            // only send items when legend necessary
            if (post.Label != "ID")
                summary.Items = null;

            if (format == "CSV")
            {
                var dtblConcordance = concordance.ToTable();
                if (dtblConcordance.Columns.IndexOf("InstrumentDescription") > -1)
                    dtblConcordance.Columns.Remove("InstrumentDescription");

                return CsvResponse(dtblConcordance);
            }
            
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            byte[] result = null;

            try
            {
                var datasets = new Dictionary<string, DataTable>
                {
                    ["ConcordanceReport"] = concordance.ToTable()
                };
                result = ReportHelper.GetReport("ConcordanceReport", format, datasets, parameters);
            }
            catch(Exception ex)
            {
                // ignore exceptions
            }
            
            if (format?.ToUpper() == "PDF")
            {
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return Ok(ResponseHelper.CompositeImageResponse(summary, webImage));
            }
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("supplyConcordanceReport")]
        [Route("supplyConcordanceReport/{format}")]
        public async Task<ActionResult> SupplyConcordanceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.CardCategoryID == null &&
                post.CardID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetSupplyConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, 
                post.CardCategoryID, post.CardID, post.Instruments, post.Label, user.SelectedLocation);

            var summary = SummarizeConcordanceReport(analytics.Tables[0], null);

            var concordance = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "usage":
                    concordance.Sort = "QtyOpen DESC, TrayUsage DESC, InstrumentDescription";
                    summary.TrayData = summary.TrayData.OrderByDescending(s => s.ItemUsage).ToList();
                    summary.Items = summary.Items.OrderByDescending(i => i.QtyOpen).ThenByDescending(i => i.TrayUsage).ThenBy(i => i.InstrumentDescription).ToList();
                    break;
                case "card_qty":
                default:
                    concordance.Sort = "TrayQty DESC, InstrumentDescription";
                    summary.TrayData = summary.TrayData.OrderByDescending(s => s.ItemQuantity).ToList();
                    summary.Items = summary.Items.OrderByDescending(i => i.TrayQty).ThenBy(i => i.InstrumentDescription).ToList();
                    break;
            }


            if (format == "CSV")
            {
                // rename columns from SSRS to human readable
                var tblConcordance = concordance.ToTable();
                tblConcordance.Columns["TrayQty"].ColumnName = "CardQty";
                tblConcordance.Columns["TrayUsage"].ColumnName = "CardUsage";

                tblConcordance.Columns.Add(new DataColumn("UnderAlloc"));
                tblConcordance.Columns.Add(new DataColumn("OverAlloc"));

                foreach (DataRow drConcordance in tblConcordance.Rows)
                {
                    var qty = drConcordance["CardQty"];
                    var usage = drConcordance["CardUsage"];
                    if (qty != DBNull.Value && usage != DBNull.Value)
                    {
                        var delta = (decimal)qty - (decimal)usage;
                        drConcordance["UnderAlloc"] = delta < 0 ? delta * -1 : 0;
                        drConcordance["OverAlloc"] = delta > 0 ? delta : 0;
                    }
                }

                return CsvResponse(tblConcordance);
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ConcordanceReport"] = concordance.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport("ConcordanceReport", format, datasets, parameters);

            if (format?.ToUpper() == "PDF")
            {
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return Ok(ResponseHelper.CompositeImageResponse(summary, webImage));
            }
        }

        private ConcordanceReportSummary SummarizeConcordanceReport(DataTable concordanceData, DataTable distributionData)
        {
            var result = new ConcordanceReportSummary()
            {
                TrayData = new List<ConcordanceReportTrayData>(),
                Items = new List<ConcordanceReportItemData>(),
                DistributionData = new List<ConcordianceDistributionData>()
            };

            foreach (DataRow concordanceRow in concordanceData.Rows)
            {
                var trayQty = concordanceRow["TrayQty"];
                var trayUsage = concordanceRow["TrayUsage"];
                var qtyOpen = concordanceRow["QtyOpen"];
                var tray = concordanceRow["SurgeonName"].ToString();

                var item = new ConcordanceReportItemData()
                {
                    ID = (string)concordanceRow["InstrumentName"],
                    InstrumentDescription = (string)concordanceRow["InstrumentDescription"],
                    QtyOpen = qtyOpen == DBNull.Value ? 0 : (decimal)qtyOpen,
                    TrayUsage = trayUsage == DBNull.Value ? 0 : (decimal)trayUsage,
                    TrayQty = trayQty == DBNull.Value ? 0 : (decimal)trayQty
                };

                var match = result.Items.FirstOrDefault(i => i.ID == item.ID);
                if (match == null)
                {
                    result.Items.Add(item);
                }
                else
                {
                    match.QtyOpen = (item.QtyOpen > match.QtyOpen ? item.QtyOpen : match.QtyOpen);
                    match.TrayUsage = (item.TrayUsage > match.TrayUsage ? item.TrayUsage : match.TrayUsage);
                    match.TrayQty = (item.TrayQty > match.TrayQty ? item.TrayQty : match.TrayQty);
                }

                if (trayQty != DBNull.Value)
                {
                    var quantity = (decimal)trayQty;
                    var usageQty = (decimal)concordanceRow["TrayUsage"];
                    var trayCases = concordanceRow["TrayCases"] == DBNull.Value ? 0 : (int)concordanceRow["TrayCases"];
                    var trayPulled = concordanceRow["TrayPulled"] == DBNull.Value ? 0 : (int)concordanceRow["TrayPulled"];
                    var trayOpened = concordanceRow["TrayOpened"] == DBNull.Value ? 0 : (int)concordanceRow["TrayOpened"];

                    var usage = result.TrayData.FirstOrDefault(r => r.TrayName == tray);
                    if (usage == null)
                    {
                        usage = new ConcordanceReportTrayData()
                        {
                            TrayName = tray,
                            TrayCases = trayCases,
                            TrayOpened = trayOpened,
                            TrayPulled = trayPulled
                        };
                        result.TrayData.Add(usage);
                    }

                    usage.TrayItems.Add(new ConcordanceItem()
                    {
                        Usage = usageQty,
                        Quantity = quantity
                    });
                }
            }

            if (distributionData == null) return result;

            foreach (DataRow distributionRow in distributionData.Rows)
            {
                var distribution = new ConcordianceDistributionData()
                {
                    TrayNames = distributionRow["tray_csv"].ToString().Split(',').ToList(),
                    CaseCount = (int)distributionRow["frequency"]
                };

                decimal netQty = (int)distributionRow["instrument_count"];
                decimal usageQty = (int)distributionRow["instrument_usage"];

                distribution.TrayQty = netQty / distribution.CaseCount;
                distribution.TrayUsage = usageQty / distribution.CaseCount;

                result.DistributionData.Add(distribution);
            }
            foreach(var distribution in result.DistributionData)
            {
                distribution.Frequency = 100 * (decimal)distribution.CaseCount / result.DistributionData.Sum(d => d.CaseCount);
            }

            return result;
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("vendorTrayConcordanceReport")]
        [Route("vendorTrayConcordanceReport/{format}")]
        public async Task<ActionResult> VendorTrayConcordanceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.CardCategoryID == null &&
                post.CardID == null &&
                post.CaseProfileId == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetVendorTrayConcordanceReportData(post.SpecialtyID, post.SurgeonID, post.ProcedureID, post.TrayID,
                post.CardCategoryID, post.CardID, post.Instruments, post.CaseProfileId, post.QuestionId, post.AnswerId, post.MetricID, user.SelectedLocation,
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

            var summary = SummarizeConcordanceReport(analytics.Tables[0], analytics.Tables[1]);

            if (format == "CSV")
            {
                return CsvResponse(concordance.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ConcordanceReport"] = concordance.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"ConcordanceReport", format, datasets, parameters);

            if (format?.ToUpper() == "PDF")
            {
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return Ok(ResponseHelper.CompositeImageResponse(summary, webImage));
            }
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("oppSummary")]
        [Route("oppSummary/{format}")]
        public async Task<ActionResult> OPPSummaryReport([FromBody] ProcedureProfileReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var procedureProfiles = await _sqlHelper.GetProcedureProfiles();
            var csvResponse = string.Empty;

            var analyticsSummary = new List<OpFlowProcedureProfileSummary>();

            foreach (var procedureProfileId in post.ProcedureProfileID)
            {
                var procedureProfile = procedureProfiles.First(p => p.ProcedureProfileID == procedureProfileId);

                var analytics = await _sqlHelper.GetProcedureProfileSummaryReportData(procedureProfileId,
                    post.SpecialtyID, post.ProcedureID, post.SurgeonID, post.MetricID, post.TrayTypeID, user.SelectedLocation);

                analytics.ProcedureProfileName = procedureProfile.ProcedureProfileName;
                analyticsSummary.Add(analytics);

                csvResponse = $"{analytics.ProcedureProfileName} Summary:\r\n";
                csvResponse += "Internal Trays\r\n";
                csvResponse += $"Tray,Instrument,OPPQty,OppUsage,LocationQty,LocationUsage,SurgeonQty,SurgeonUsage,Reduction\r\n";
                foreach (var item in analytics.InternalTrays)
                {
                    csvResponse += $"{item.ContainerName},{item.ItemName},{item.OPPQty},{item.OppUsage},{item.LocationQty},{item.LocationUsage},{item.LocationQty},{item.SurgeonUsage},{item.Reduction}\r\n";
                }

                csvResponse += "\r\n\r\nVendor Trays\r\n";
                csvResponse += $"Tray,Instrument,OPPQty,OppUsage,LocationQty,LocationUsage,SurgeonQty,SurgeonUsage,Reduction\r\n";
                foreach (var item in analytics.VendorTrays)
                {
                    csvResponse += $"{item.ContainerName},{item.ItemName},{item.OPPQty},{item.OppUsage},{item.LocationQty},{item.LocationUsage},{item.LocationQty},{item.SurgeonUsage},{item.Reduction}\r\n";
                }

                csvResponse += "\r\n\r\nDisposables\r\n";
                csvResponse += $"Card,Instrument,OPPQty,OppUsage,LocationQty,LocationUsage,SurgeonQty,SurgeonUsage,Reduction\r\n";
                foreach (var item in analytics.Items)
                {
                    csvResponse += $"{item.ContainerName},{item.ItemName},{item.OPPQty},{item.OppUsage},{item.LocationQty},{item.LocationUsage},{item.LocationQty},{item.SurgeonUsage},{item.Reduction}\r\n";
                }
                csvResponse += "\r\n";

            }
            if (format == "CSV")
            {

                return CsvResponse(csvResponse);
            }
            
            return Ok(
                new
                {
                    Summary = analyticsSummary,                    
                });
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("supplyWaste")]
        [Route("supplyWaste/{format}")]
        public async Task<ActionResult> SupplyWasteReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.StartDate == null &&
                post.EndDate == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetSupplyWasteReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID, post.CardCategoryID,
                post.MinCost, post.MinOpen, post.MinHold, post.StartDate, post.EndDate, true, true, true, true, post.Group, user.SelectedLocation);

            var supplyWaste = analytics.Tables[0].DefaultView;
            
            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyWaste"] = supplyWaste.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"SupplyWaste{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("serviceLineReview")]
        [Route("serviceLineReview/{format}")]
        public async Task<ActionResult> ServiceLineReviewReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.StartDate == null &&
                post.EndDate == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            if (format?.ToUpper() == "CSV")
            {
                // override some of the parameters to force more detail
                post.Group = "SCI";
            }

            var analytics = await _sqlHelper.GetServiceLineReviewReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID,
                post.CardCategoryID, post.MinCost, post.MinOpen, post.MinHold, 
                post.FieldAll, post.FieldWaste, post.FieldOver, post.FieldUnder,
                post.StartDate, post.EndDate, post.Group, user.SelectedLocation);

            var supplyOpen = analytics.Tables[0].DefaultView;
            var supplyOpenAggregate = analytics.Tables[1].DefaultView;

            if (format?.ToUpper() == "CSV")
            {
                return CsvResponse(supplyOpen.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["ServiceLineReview"] = supplyOpenAggregate.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"ServiceLineReview{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

            if (format?.ToUpper() == "PDF")
            {
                return File(result, "application/octet-stream");
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

                return Ok(ResponseHelper.CompositeImageResponse(summarySpecialties, webImage));
            }
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("supplyOpen")]
        [Route("supplyOpen/{format}")]
        public async Task<ActionResult> SupplyOpenReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.StartDate == null &&
                post.EndDate == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            if (format?.ToUpper() == "CSV")
            {
                // override some of the parameters to force more detail
                post.Group = "SCI";
            }

            var analytics = await _sqlHelper.GetSupplyWasteReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID, post.CardCategoryID,
                post.MinCost, post.MinOpen, post.MinHold, post.StartDate, post.EndDate, 
                post.FieldAll, post.FieldWaste, post.FieldOver, post.FieldUnder,
                post.Group, user.SelectedLocation);

            var supplyOpen = analytics.Tables[0].DefaultView;
            var supplyOpenAggregate = analytics.Tables[1].DefaultView;
            
            if (format?.ToUpper() == "CSV")
            {
                var userObject = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);

                var dtSupply = supplyOpen.ToTable();
                dtSupply.Columns.Add("MRN", typeof(string));

                var extract = "MRN,SurgeryDate,SurgeonName,ItemName,Specialty,ProcedureName,ItemCost,CardName,CardQuantity<TotalUsed,SetupOpen,SetupAdded,QtyOpenInit,QtyOpenAdded,OpenVariance,UsageVariance,CardQtyVariance,TotalOpen";
                
                foreach (DataRow dtSupplyData in dtSupply.Rows)
                {
                    extract += "\r\n";

                    var patientId = (int)dtSupplyData["PatientID"];
                    
                    var surgeryDate = ((DateTime)dtSupplyData["SurgeryDate"]).ToShortDateString();
                    var surgeon = dtSupplyData["SurgeonName"].ToString().Trim().Replace("\"", "\"\"");
                    var item = dtSupplyData["ItemName"].ToString().Trim().Replace("\"", "\"\"");
                    var specialty = dtSupplyData["Specialty"].ToString().Trim().Replace("\"", "\"\"");
                    var procName = dtSupplyData["ProcedureName"].ToString().Trim().Replace("\"", "\"\"");
                    var cardName = dtSupplyData["CardName"].ToString().Trim().Replace("\"", "\"\"");

                    extract += $"=\"{0000}\",{surgeryDate},\"{surgeon}\",\"{item}\",\"{specialty}\",\"{procName}\"," +
                        $"{dtSupplyData["ItemCost"]},\"{cardName}\",{dtSupplyData["CardQuantity"]},{dtSupplyData["TotalUsed"]},{dtSupplyData["SetupOpen"]},{dtSupplyData["SetupAdded"]}" +
                        $"{dtSupplyData["QtyOpenInit"]},{dtSupplyData["QtyOpenAdded"]},{dtSupplyData["OpenVariance"]},{dtSupplyData["UsageVariance"]},{dtSupplyData["CardQtyVariance"]},{dtSupplyData["TotalOpen"]}";
                }

                return CsvResponse(extract);
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
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"SupplyOpen{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("supplyUsageVariance")]
        [Route("supplyUsageVariance/{format}")]
        public async Task<ActionResult> SupplyUsageVarianceReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetSupplyWasteReportData(post.SpecialtyID, post.SurgeonID, post.CardID, post.ItemID, post.CardCategoryID,
                post.MinCost, post.MinOpen, post.MinHold, post.StartDate, post.EndDate, true, true, true, true, post.Group, user.SelectedLocation);

            var supplyOpen = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyUsageVariance"] = supplyOpen.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"SupplyUsageVariance{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("cardRedundancy")]
        [Route("cardRedundancy/{format}")]
        public async Task<ActionResult> CardRedundancyReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetCardRedundancyReport(post.SpecialtyID, post.SurgeonID, post.CardID, post.MinQty, post.Redundancy, user.SelectedLocation);

            var cardRedundancy = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["CardRedundancy"] = cardRedundancy.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"CardRedundancy{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("procedureRedundancy")]
        [Route("procedureRedundancy/{format}")]
        public async Task<ActionResult> ProcedureRedundancyReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ProcedureID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetProcedureRedundancyReport(post.SpecialtyID, post.SurgeonID, post.CardID, post.ProcedureID, post.MinQty, post.Redundancy, user.SelectedLocation);

            var procedureRedundancy = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["ProcedureRedundancy"] = procedureRedundancy.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"ProcedureRedundancy{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("excessInventory")]
        [Route("excessInventory/{format}")]
        public async Task<ActionResult> ExcessInventoryReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.ProposedTrayID == null && post.SpecialtyID == null && post.TrayStatus == null && post.TrayPhaseID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetExcessInventoryReport(post.SpecialtyID, post.ProposedTrayID, post.TrayStatus, post.TrayPhaseID,                 
                post.Group, user.SelectedLocation);


            if (format == "CSV")
            {
                // adjust CSV export to remove formatting needed on RDL
                foreach (DataRow dataRow in analytics.Tables[0].Rows)
                {
                    var category = dataRow["InstrumentCategory"].ToString();
                    if (string.IsNullOrEmpty(category))
                        category = "No Category";

                    var instrumentName = dataRow["InstrumentName"].ToString();
                    instrumentName = instrumentName.Replace($"{category} - ", "");
                    dataRow["InstrumentName"] = instrumentName;
                }

                return CsvResponse(analytics.Tables[0]);
            }

            var excessInventory = analytics.Tables[0].DefaultView;
            var datasets = new Dictionary<string, DataTable>
            {
                ["ExcessInventory"] = excessInventory.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"ExcessInventory{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("supplyCost")]
        [Route("supplyCost/{format}")]
        public async Task<ActionResult> SupplyCostReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.CardCategoryID == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.CardID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetSupplyCardCostReportData(post.SpecialtyID, post.SurgeonID, post.CardCategoryID, 
                user.SelectedLocation);

            var supplyWaste = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyCost"] = supplyWaste.ToTable()
            };
            var parameters = new ReportParameter[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"SupplyCost{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("supplySavings")]
        [Route("supplySavings/{format}")]
        public async Task<ActionResult> SupplySavingsEstimatorReport([FromBody] InstrumentUsagePost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            if (post.CardCategoryID == null &&
                post.SpecialtyID == null &&
                post.SurgeonID == null &&
                post.ItemID == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetSupplySavingsEstimatorReportData(post.SpecialtyID, post.SurgeonID, post.CardCategoryID,
                post.ItemID, user.SelectedLocation);

            var supplySavings = analytics.Tables[0].DefaultView;

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplySavings"] = supplySavings.ToTable()
            };
            var parameters = new ReportParameter[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"SupplySavingsEstimator{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

            if (format?.ToUpper() == "PDF")
            {
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);
                var summary = SummarizeSupplySavings(supplySavings.ToTable());

                return Ok(ResponseHelper.CompositeImageResponse(summary, webImage));
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
        [HttpPut]
        [HttpPost]
        [Route("trayConsolidation")]
        [Route("trayConsolidation/{format}")]
        public async Task<ActionResult> TrayConsolidationReport([FromBody] TrayConsolidationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsTrayConsolidationData(post.SpecialtyId, post.TrayId, post.Reallocation,
                post.MaxSize, post.MinCards, post.MinConsolidationInstances, post.MinTargetInstances, post.Overlap, post.Effect, post.ProcedureGroup, post.Group, user.SelectedLocation);
            
            if (format?.ToUpper() == "CSV")
            {
                var result = "Group,TrayName,Cards,Instruments,Instances,Audits,Counts,TrayAvgUsage\r\n";

                foreach (var group in analytics)
                {
                    foreach (var c in group.Consolidations)
                    {
                        result += $"\"{group.GroupName}\",\"{c.TrayName}\",{c.CardCount},{c.TrayInstrumentCount},{c.TrayInstances}" +
                            $"{c.TrayAudits},{c.TrayCounts},{c.TrayAvgUsage}\r\n";
                    }
                }

                return CsvResponse(result);
            }

            return Ok(analytics);
            /*
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
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return Ok(ResponseHelper.ImageResponse(webImage));
            }*/
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("trayReductionSummary")]
        [Route("trayReductionSummary/{format}")]
        public async Task<ActionResult> TrayReductionSummaryReport([FromBody] TrayConsolidationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsTrayReductionSummaryData(post.SpecialtyId, user.SelectedLocation);

            var reduction = new DataView(analytics.Tables[0]);

            if (format == "CSV")
            {
                return CsvResponse(reduction.ToTable());
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayReductionSummary"] = reduction.ToTable()
            };


            var parameters = new ReportParameter[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var result = ReportHelper.GetReport($"TrayReductionSummary{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("trayConsolidationDownload")]
        public async Task<ActionResult> TrayConsolidationDownload([FromBody] TrayConsolidationExportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var export = "Tray 1, Tray 2, Shared Card\r\n";

            foreach (var exportTray in post.Exports)
            {
                var cards = await _sqlHelper.GetTrayCards(exportTray.TrayID, user.SelectedLocation);

                if (exportTray.Children == null || !exportTray.Children.Any())
                {
                    foreach (var card in cards)
                    {
                        export += $"{card.TrayName},NULL,{card.CardDescription}\r\n";
                    }
                }

                foreach (var child in exportTray?.Children ?? new List<TrayConsolidationExport>())
                {
                    var cards2 = await _sqlHelper.GetTrayCards(child.TrayID, user.SelectedLocation);

                    foreach (var card2 in cards2.Where(c2 => cards.Any(c => c2.CardID == c.CardID)))
                    {
                        var card1 = cards.FirstOrDefault(c => c.CardID == card2.CardID);
                        export += $"{card1?.TrayName},{card2.TrayName},{card2.CardDescription}\r\n";
                    }
                    
                    foreach (var child2 in child?.Children ?? new List<TrayConsolidationExport>())
                    {
                        var cards3 = await _sqlHelper.GetTrayCards(child2.TrayID, user.SelectedLocation);

                        foreach (var card3 in cards3.Where(c3 => cards2.Any(c2 => c3.CardID == c2.CardID)))
                        {
                            var card2 = cards2.FirstOrDefault(c => c.CardID == card3.CardID);
                            export += $"{card2.TrayName},{card3.TrayName},{card3.CardDescription}\r\n";
                        }
                    }
                }
            }

            var extractBytes = System.Text.Encoding.UTF8.GetBytes(export);

            return File(extractBytes, "application/octet-steam");
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("trayRationalization")]
        [Route("trayRationalization/{format}")]
        public async Task<ActionResult> TrayRationalizationReport([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, post.TrayTypeId, post.VendorTray,
                post.CardCategoryId, post.CardId, post.MetricId, post.MinSize, user.SelectedLocation);

            var rationalization = new DataView(analytics.Tables[0]);
            
            switch (post.Order)
            {
                case "instrument_nbr":
                    rationalization.Sort = "InstrumentCount";
                    break;
                case "instrument_avg":
                    rationalization.Sort = "UsageQuantity";
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
                new ReportParameter("FieldTray", post.FieldTray.ToString()),
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayRationalization"] = rationalization.ToTable()
            };
            var result = ReportHelper.GetReport($"TrayRationalization{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);
            
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
        [HttpPut]
        [HttpPost]
        [Route("vendorTrayRationalization")]
        [Route("vendorTrayRationalization/{format}")]
        public async Task<ActionResult> VendorTrayRationalizationReport([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsVendorTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, post.CardCategoryId,
                post.MinSize, post.StartDate, post.EndDate, post.CaseProfileId, post.QuestionId, post.AnswerId, post.MetricId, user.SelectedLocation);

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
                new ReportParameter("FieldTray", post.FieldTray.ToString()),
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var result = ReportHelper.GetReport($"TrayRationalization{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("trayScope")]
        [Route("trayScope/{format}")]
        public async Task<ActionResult> TrayScopeReport([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsTrayScopeData(post.SpecialtyId, post.TrayId, post.Group, user.SelectedLocation);

            var rationalization = new DataView(analytics.Tables[0]);

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayScope"] = rationalization.ToTable()
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"TrayScope{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("trayRationalizationImage")]
        [Route("trayRationalizationImage/{format}")]
        public async Task<ActionResult> TrayRationalizationReportImage([FromBody] TrayRationalizationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsTrayRationalizationData(post.SpecialtyId, post.SurgeonId, post.TrayId, post.TrayTypeId, post.VendorTray, post.CardCategoryId, post.CardId,
                post.MetricId, post.MinSize, user.SelectedLocation);
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
                return File(result, "application/octet-stream");
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return Ok(ResponseHelper.ImageResponse(webImage));
            }
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("countSummary")]
        [Route("countSummary/{format}")]
        public async Task<ActionResult> CountSummaryReport([FromBody] CountSummaryReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAnalyticsCountSummaryData(post.SpecialtyId, post.SurgeonId, post.CardId, post.CardCategoryId,
                post.RoomGroupId, post.TrayId, post.CardType, post.Group, user.SelectedLocation);

            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var countSummary = analytics.Tables[0].DefaultView;
            switch (post.Order)
            {
                case "specialty":
                    countSummary.Sort = "Specialty";
                    break;
                case "card":
                    countSummary.Sort = "CardCount";
                    break;
                case "count":
                default:
                    countSummary.Sort = "InstrumentCount";
                    break;
            }

            if (format?.ToUpper() == "CSV")
            {
                var dtblCountSummary = countSummary.ToTable();
                return CsvResponse(dtblCountSummary);
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["CountSummary"] = countSummary.ToTable()
            };
            var result = ReportHelper.GetReport($"CountSummary{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("countSampleDispersion")]
        [Route("countSampleDispersion/{format}")]
        public async Task<ActionResult> CountSampleDispersionReport([FromBody] CountSampleDispersionReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            
            var analytics = await _sqlHelper.GetAnalyticsCountSampleDispersion(post.CountType, post.SpecialtyId, post.SurgeonId, post.TrayId, post.ItemId,
                post.CardCategoryId, post.StartDate, post.EndDate, post.Group, user.SelectedLocation);

            string groupType;
            switch (post.Group)
            {
                case "P":
                    groupType = "Pref Card";
                    break;
                case "G":
                    groupType = "Procedure";
                    break;
                case "S":
                default:
                    groupType = "Specialty";
                    break;
            }

            if (format?.ToUpper() == "PDF")
            {
                var datasets = new Dictionary<string, DataTable>
                {
                    ["CountSampleDispersion"] = analytics.Tables[0]
                };
                var parameters = new[]
                {
                    new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                    new ReportParameter("Timezone", post.Timezone.ToString())
                };
                var result = ReportHelper.GetReport("CountSampleDispersion", format, datasets, parameters);

                return File(result, "application/octet-stream");
            }
            else
            {
                var summary = analytics.Tables[0].DataTableToList<CountSampleDispersionReport>();
                var velocity = analytics.Tables[1].DataTableToList<CountSampleVelocity>();

                var cards = summary.GroupBy(s => new { s.CardId, s.Card, s.Surgeon, s.GroupValue }).Select(s =>
                    new CountSampleDispersionReportCard()
                    {
                        Surgeon = s.Key.Surgeon,
                        GroupValue = s.Key.GroupValue,
                        Card = s.Key.Card,
                        CardCount = s.Max(c => c.CardCount),
                        Velocity = velocity.FirstOrDefault(v => v.CardId == s.Key.CardId),
                        //Instruments = s.ToList()
                    });
                var surgeons = cards.GroupBy(c => new { c.Surgeon, c.GroupValue }).Select(s =>
                    new CountSampleDispersionReportSurgeon()
                    {
                        Surgeon = s.Key.Surgeon,
                        GroupValue = s.Key.GroupValue,
                        Cards = s.ToList()
                    });

                return Ok(new{
                    GroupType = groupType,
                    Surgeons = surgeons
                });
            }
        }

        // GET api/values/5
        [HttpPut]
        [HttpPost]
        [Route("procedureMix")]
        [Route("procedureMix/{format}")]
        public async Task<ActionResult> ProcedureMixReport([FromBody] CountSampleDispersionReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            
            var analytics = await _sqlHelper.GetAnalyticsProcedureMix(post.SpecialtyId, post.SurgeonId, post.TrayId, post.ItemId,
                post.CardCategoryId, user.SelectedLocation);

            var datasets = new Dictionary<string, DataTable>
            {
                ["ProcedureMix"] = analytics.Tables[0]
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"ProcedureMix{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("trayProcedureMix")]
        [Route("trayProcedureMix/{format}")]
        public async Task<ActionResult> TrayProcedureMixReport([FromBody] CountSampleDispersionReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            
            var analytics = await _sqlHelper.GetAnalyticsTrayProcedureMix(post.SpecialtyId, post.SurgeonId, post.TrayId, 
                post.CardCategoryId, post.MetricId, post.MinTray, post.TrayTypeID, post.VendorTray, user.SelectedLocation);

            var datasets = new Dictionary<string, DataTable>
            {
                ["TrayProcedureMix"] = analytics.Tables[0]
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"TrayProcedureMix{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("supplyCount")]
        [Route("supplyCount/{format}")]
        public async Task<ActionResult> SupplyCountReport([FromBody] CountSummaryReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            
            var analytics = await _sqlHelper.GetAnalyticsSupplyCountData(post.SpecialtyId, post.SurgeonId, post.CardId, post.CardCategoryId,
                post.Group, user.SelectedLocation);

            if (format?.ToUpper() == "CSV")
            {
                // pivot export to be items by surgeons
                var itemUsageCollection = new Dictionary<string, Dictionary<string, int>>();
                var categories = new List<string>();
                foreach (DataRow drSummary in analytics.Tables[0].Rows)
                {
                    var category = drSummary["CategoryName"].ToString();
                    var itemName = drSummary["ItemName"].ToString();
                    var itemCount = (int)drSummary["InstrumentCount"];
                    if (!categories.Contains(category))
                    {
                        categories.Add(category);
                    }
                    if (!itemUsageCollection.ContainsKey(itemName))
                    {
                        var itemUsage = new Dictionary<string, int>();
                        itemUsageCollection[itemName] = itemUsage;
                    }

                    itemUsageCollection[itemName][category] = itemCount;
                }

                var csvResult = "Item Name," + string.Join(",", categories) + "\r\n";
                foreach (var itemUsage in itemUsageCollection)
                {
                    csvResult += $"\"{itemUsage.Key}\"";
                    var itemUsages = itemUsage.Value;
                    foreach (var category in categories)
                    {
                        var categoryUsage = itemUsage.Value.ContainsKey(category) ? itemUsage.Value[category] : 0;
                        csvResult += $",{categoryUsage}";
                    }
                    csvResult += "\r\n";
                }
                return CsvResponse(csvResult);
            }

            var datasets = new Dictionary<string, DataTable>
            {
                ["SupplyCount"] = analytics.Tables[0]
            };
            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };
            var result = ReportHelper.GetReport($"SupplyCount{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

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
        [HttpPut]
        [HttpPost]
        [Route("countDistribution")]
        [Route("countDistribution/{format}")]
        public async Task<ActionResult> CountDistributionReport([FromBody] SupplyDistributionPost post, string format = null)
        {
            var user = await GetUserSecurity();

            format = format ?? "IMAGE";

            
            var analytics = await _sqlHelper.GetAnalyticsCountDistributionData(post.SpecialtyId, post.SurgeonId, post.CardCategoryId, 
                post.ItemCategoryId, post.ItemId, post.MinCost, user.SelectedLocation);

            var parameters = new[]
            {
                new ReportParameter("card_filter", post.CardFilter),
                new ReportParameter("surgeon_filter", post.SurgeonFilter),
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            if (format?.ToUpper() == "PDF")
            {

                var datasets = new Dictionary<string, DataTable>
                {
                    ["SupplyDistribution"] = analytics.Tables[0]
                };
                var result = ReportHelper.GetReport($"CountDistribution{(format == "IMAGE" ? "" : "Export")}", format, datasets, parameters);

                return File(result, "application/octet-stream");
            }
            else
            {
                var summary = SummarizeDistribution(analytics.Tables[0], post.CardFilter, post.SurgeonFilter);
                return Ok(summary);
            }
        }

        private CountDistributionSummary SummarizeDistribution(DataTable dtblDistribution, string cardFilter, string surgeonFilter)
        {
            var result = new CountDistributionSummary();
            
            result.Supplies = dtblDistribution
                .AsEnumerable()
                .Select(r => r.Field<string>("ItemName"))
                .Distinct()
                .Count();

            var summary = dtblDistribution.DataTableToList<CountDistributionOutput>();
            var validSurgeons = new List<string>();
            foreach (var surgeon in summary.GroupBy(s => s.Surgeon))
            {
                if (surgeon.Sum(s => s.CardCount) > 0)
                {
                    result.SurgeonCounts++;
                    if (surgeonFilter == "C")
                        validSurgeons.Add(surgeon.Key);
                }
                else
                {
                    result.SurgeonNoCounts++;
                    if (surgeonFilter == "N")
                        validSurgeons.Add(surgeon.Key);
                }

                foreach (var card in surgeon.ToList().GroupBy(c => c.Card))
                {
                    if (card.Sum(s => s.CardCount) > 0)
                        result.CardCounts++;
                    else
                        result.CardNoCounts++;
                }
            }

            var type = summary.GroupBy(s => s.CountType);
            result.Data = type.Select(t => new CountDistributionReport()
            {
                CountType = t.Key,
                Data = t.GroupBy(ct => ct.ProcedureGroup).Select(ct =>
                    new CountDistributionReportType()
                    {
                        ProcedureGroup = ct.Key,
                        Data = ct.GroupBy(pg => pg.Specialty).Select(pg =>
                        new CountDistributionReportProcedureGroup()
                        {
                            ServiceLine = pg.Key,
                            Data = pg.GroupBy(s => s.Surgeon).Select(s =>
                            new CountDistributionReportSpecialty()
                            {
                                Surgeon = s.Key,
                                Data = s.GroupBy(c => c.Card).Select(c =>
                                new CountDistributionReportSurgeon()
                                {
                                    Card = c.Key,
                                    Data = c.Select(d =>
                                        new CountDistributionReportCard()
                                        {
                                            ItemName = d.ItemName,
                                            CardCount = d.CardCount,
                                            UnitCost = d.UnitCost,
                                            CardQty = d.CardQty,
                                            Setup = d.Setup,
                                            Usage = d.Usage,
                                            Added = d.Added
                                        }).ToList()
                                }).Where(d => cardFilter == "A" ||
                                (d.CardCount == 0 && cardFilter == "N") ||
                                (d.CardCount > 0 && cardFilter == "C")).ToList()
                            }).Where(s => surgeonFilter == "A" ||
                                validSurgeons.Contains(s.Surgeon)).ToList()
                    }).ToList()
                }).ToList()
            }).ToList();

            return result;
        }
    }
}
