using System;
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
using OpFlow.Data.Administration;
using OpFlow.Data.Analytics;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/administration")]
    public class AdministrationController : OpFlowController
    {
        public AdministrationController(IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        [Route("trayHistory")]
        [HttpPost]
        public async Task<ActionResult> VendorTrayHistory([FromBody] TrayHistoryRequest post, 
            int? specialtyId = null, int? userId = null, int? cardId = null,
            DateTime? beginDate = null, DateTime? endDate = null, int? itemId = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var questions = post?.Questions ?? new List<TrayQuestion>();

            var trayHistory = await _sqlHelper.GetTrayHistory(specialtyId, userId, cardId, beginDate, endDate, itemId, questions, user.ProviderID, user.LocationID);

            return Ok(trayHistory);
        }

        // GET api/values/5
        [Route("importTypes")]
        public async Task<ActionResult> GetImportTypes()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var importTypes = await _sqlHelper.GetImportTypes(user.ProviderID, user.LocationID);

            return Ok(importTypes);
        }

        // GET api/values/5
        [Route("importDetails")]
        public async Task<ActionResult> GetImportDetail(int importTypeId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var result = new ImportDetail()
            {
                ImportDefinitions = await _sqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID),
                ImportLogs = await _sqlHelper.GetImportLog(importTypeId, user.SelectedLocation)
            };

            return Ok(result);
        }

        // GET api/values/5
        [Route("importMessages")]
        public async Task<ActionResult> GetImportMessages(int importLogId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var result = await _sqlHelper.GetImportMessages(importLogId, user.SelectedLocation);

            return Ok(result);
        }

        // GET api/values/5
        [Route("sample/{importTypeId}")]
        public async Task<ActionResult> GetSample(int importTypeId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var columns = await _sqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID);

            var sample = string.Empty;
            var strDelim = string.Empty;
            foreach (var column in columns)
            {
                sample += $"{strDelim}{column.ColumnName}";
                strDelim = ",";
            }
            sample += "\r\n";

            strDelim = string.Empty;
            foreach (var column in columns)
            {
                var dataField = string.Empty;

                if (column.DataType?.ToLower()?.Contains("varchar") == true)
                {
                    dataField = "ABC";
                }

                if (column.DataType?.ToLower()?.Contains("int") == true)
                {
                    dataField = "5";
                }

                if (column.DataType?.ToLower()?.Contains("decimal") == true)
                {
                    dataField = "1.23";
                }

                if (column.DataType?.ToLower() == "date")
                {
                    dataField = "1/1/1990";
                }

                if (column.DataType?.ToLower() == "time")
                {
                    dataField = "11:30";
                }

                if (column.DataType?.ToLower() == "bool")
                {
                    dataField = "Y";
                }

                sample += $"{strDelim}{dataField}";
                strDelim = ",";
            }
            sample += "\r\n";


            var extractBytes = System.Text.Encoding.UTF8.GetBytes(sample);
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

            return Ok(result);
        }
        // GET api/values/5
        [Route("exportData")]
        public async Task<ActionResult> GetExportData()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var users = await _sqlHelper.GetSurgeons(null, user.SelectedLocation);
            var trays = await _sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation);
            var cardCategories = await _sqlHelper.GetCardCategories();
            var locations = await _sqlHelper.GetUserLocations(user.UserID, user.SelectedLocation);

            return Ok(new
            {
                Specialties = specialties,
                Users = users,
                Trays = trays,
                CardCategories = cardCategories,
                Locations = locations
            });
        }

        // GET api/values/5
        [Route("export/{exportType}")]
        [HttpPut]
        public async Task<ActionResult> PutExport(string exportType, [FromBody] DataExportPost request)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            System.Data.DataSet dsUsage;
            
            switch (exportType)
            {
                case "COUNT-SUMMARY":
                case "COUNT-DATA":
                    dsUsage = await _sqlHelper.GetExportCounts(request.StartDate, request.EndDate, 
                        request.SpecialtyID, request.UserID, request.CardID, request.ItemID, request.CountType, exportType.Contains("DATA"), user.SelectedLocation);
                    break;
                case "USAGE":
                    dsUsage = await _sqlHelper.GetExportUsage(request.StartDate, request.EndDate,
                        request.SpecialtyID, request.UserID, request.CardID, request.ItemID, request.CardCategoryID, user.SelectedLocation);
                    break;
                case "TRAY":
                    dsUsage = await _sqlHelper.GetExportTray(request.SpecialtyID, request.UserID, request.CardID, request.ItemID, user.SelectedLocation);
                    break;
                case "CARD":
                    dsUsage = await _sqlHelper.GetExportCard(request.SpecialtyID, request.UserID, request.CardID, user.SelectedLocation);
                    break;
                default:
                    return CsvResponse("Unknown export type");
            }

            var dataExport = dsUsage.Tables[0].DefaultView;

            if (request.IncludeColumns?.Any() == true)
            {
                var idx = 0;
                var columnCount = dataExport.Table.Columns.Count;
                while (idx < columnCount)
                {
                    var columnName = dataExport.Table.Columns[idx].ColumnName;
                    if (request.IncludeColumns.Any(c => c.Equals(columnName, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        idx++;
                    }
                    else
                    {
                        dataExport.Table.Columns.RemoveAt(idx);
                    }

                    columnCount = dataExport.Table.Columns.Count;
                }
            }

            return CsvResponse(dataExport.ToTable());
        }

        // GET api/values/5
        [Route("caseOverview")]
        public async Task<ActionResult> GetCaseOverview(DateTime beginDate, DateTime endDate, int? specialtyId = null, int? bundleId = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var result = await _sqlHelper.GetCaseOverview(user.ProviderID, user.LocationID, beginDate, endDate, specialtyId, bundleId);

            return Ok(result);
        }
        
        [HttpPut]
        [Route("importFile")]
        public async Task<ActionResult> PutImportFile(IFormFile file, int importTypeId)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            int? logId = null;

            try
            {
                // extract file name and file contents
                var fileName = file.FileName;
                byte[] fileContents;
                using (var stream = file.OpenReadStream())
                using (var memStream = new MemoryStream())
                {
                    stream.CopyTo(memStream);

                    fileContents = memStream.ToArray();
                }

                var fileParser = new FileParser(fileName, fileContents);

                await fileParser.ParseFile(_sqlHelper, importTypeId, user.SelectedLocation);

                logId = await _sqlHelper.InsertImportLog(user.SelectedLocation, importTypeId, user.UserID, fileParser.Records.Count, fileName);

                if (fileParser.Records == null)
                    return Ok(fileParser.Status);

                var secureUser = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);
                ImportResult result;

                if (importTypeId == 5 || importTypeId == 8)
                {
                    result = await _sqlHelper.UpdateCardItemImport(fileParser.Records.Select(r => r as CardImport).ToList(),
                        fileParser.Relations, user.SelectedLocation);

                    foreach (var message in result.Messages)
                    {
                        await _sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message, null, null);
                    }
                }
                else
                {
                    foreach (var record in fileParser.Records)
                    {
                        result = await _sqlHelper.InsertStagingData(user.SelectedLocation, 0, record, fileParser.Relations, user.UserID);
                        foreach (var message in result.Messages)
                        {
                            await _sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message,
                                (record as ScheduleImport)?.MRN, (record as ScheduleImport)?.ScheduleDate);
                        }
                    }
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
        [HttpGet]
        [Route("reports")]
        public async Task<ActionResult> GetReports()
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            var locations = await _sqlHelper.GetUserLocations(user.UserID, user.LocationID);
            var specialties = await _sqlHelper.GetSpecialtyMaster();
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
                    Locations = locations,
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
        [Route("procedureMix")]
        [Route("procedureMix/{format}")]
        public async Task<ActionResult> ProcedureMixReport([FromBody] AdministrationReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();

            format = format ?? "IMAGE";

            var analytics = await _sqlHelper.GetAdministrationProcedureMix(post.SpecialtyId, post.TrayId, post.ItemId,
                post.CardCategoryId, post.LocationId);

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
        [Route("trayConcordance")]
        [Route("trayConcordance/{format}")]
        public async Task<ActionResult> TrayConcordanceReport([FromBody] AdministrationUsageReportPost post, string format = null)
        {
            var user = await GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return NotFound();


            format = format ?? "IMAGE";

            if (post.SpecialtyID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.TrayTypeID == null &&
                post.CardCategoryID == null &&
                post.MetricID == null &&
                post.LocationId == null)
            {
                return Ok(new { Error = true });
            }

            var analytics = await _sqlHelper.GetAdministrationConcordanceReportData(post.SpecialtyID, post.ProcedureID, post.TrayID, post.TrayTypeID, post.VendorTray,
                post.CardCategoryID, null, post.MetricID, post.Instruments, post.ShowMax, post.Label, post.LocationId);

            var summary = SummarizeConcordanceReport(analytics.Tables[0]);

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
                return CsvResponse(concordance.ToTable());
            }

            var parameters = new[]
            {
                new ReportParameter("Target", format == "IMAGE" ? "" : await GetLocationName(user)),
                new ReportParameter("Timezone", post.Timezone.ToString())
            };

            var datasets = new Dictionary<string, DataTable>
            {
                ["ConcordanceReport"] = concordance.ToTable()
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

        private ConcordanceReportSummary SummarizeConcordanceReport(DataTable concordanceData)
        {
            var result = new ConcordanceReportSummary()
            {
                TrayData = new List<ConcordanceReportTrayData>(),
                Items = new List<ConcordanceReportItemData>()
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

            return result;
        }
    }
}