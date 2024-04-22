using System;
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
using Microsoft.ReportingServices.Diagnostics.Internal;
using OpFlow.Data;
using OpFlow.Data.Administration;
using OpFlow.Data.Analytics;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/administration")]
    public class AdministrationController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("VendorTrayHistory")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<TrayHistory>))]
        [Route("trayHistory")]
        [HttpPost]
        public async Task<HttpResponseMessage> VendorTrayHistory([FromBody] TrayHistoryRequest post, 
            int? specialtyId = null, int? userId = null, int? cardId = null,
            DateTime? beginDate = null, DateTime? endDate = null, int? itemId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var questions = post?.Questions ?? new List<TrayQuestion>();

            var sqlHelper = new SqlHelper();
            var trayHistory = await sqlHelper.GetTrayHistory(specialtyId, userId, cardId, beginDate, endDate, itemId, questions, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, trayHistory);
        }

        // GET api/values/5
        [SwaggerOperation("GetImportType")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportType>))]
        [Route("importTypes")]
        public async Task<HttpResponseMessage> GetImportTypes()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var importTypes = await sqlHelper.GetImportTypes(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, importTypes);
        }

        // GET api/values/5
        [SwaggerOperation("GetImportDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ImportDetail))]
        [Route("importDetails")]
        public async Task<HttpResponseMessage> GetImportDetail(int importTypeId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var result = new ImportDetail()
            {
                ImportDefinitions = await sqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID),
                ImportLogs = await sqlHelper.GetImportLog(importTypeId, user.SelectedLocation)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetImportMessages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportMessage>))]
        [Route("importMessages")]
        public async Task<HttpResponseMessage> GetImportMessages(int importLogId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var result = await sqlHelper.GetImportMessages(importLogId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetSample")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportMessage>))]
        [Route("sample/{importTypeId}")]
        public async Task<HttpResponseMessage> GetSample(int importTypeId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var columns = await sqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID);

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

            return result;
        }
        // GET api/values/5
        [SwaggerOperation("GetExportData")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportMessage>))]
        [Route("exportData")]
        public async Task<HttpResponseMessage> GetExportData()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();

            var specialties = await sqlHelper.GetSpecialties(user.SelectedLocation);
            var users = await sqlHelper.GetSurgeons(null, user.SelectedLocation);
            var trays = await sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation);
            var cardCategories = await sqlHelper.GetCardCategories();
            var locations = await sqlHelper.GetUserLocations(user.UserID, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, new
            {
                Specialties = specialties,
                Users = users,
                Trays = trays,
                CardCategories = cardCategories,
                Locations = locations
            });
        }

        // GET api/values/5
        [SwaggerOperation("PutExport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportMessage>))]
        [Route("export/{exportType}")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutExport(string exportType, [FromBody] DataExportPost request)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            System.Data.DataSet dsUsage;
            
            switch (exportType)
            {
                case "COUNT-SUMMARY":
                case "COUNT-DATA":
                    dsUsage = await sqlHelper.GetExportCounts(request.StartDate, request.EndDate, 
                        request.SpecialtyID, request.UserID, request.CardID, request.ItemID, request.CountType, exportType.Contains("DATA"), user.SelectedLocation);
                    break;
                case "USAGE":
                    dsUsage = await sqlHelper.GetExportUsage(request.StartDate, request.EndDate,
                        request.SpecialtyID, request.UserID, request.CardID, request.ItemID, request.CardCategoryID, user.SelectedLocation);
                    break;
                case "TRAY":
                    dsUsage = await sqlHelper.GetExportTray(request.SpecialtyID, request.UserID, request.CardID, request.ItemID, user.SelectedLocation);
                    break;
                case "CARD":
                    dsUsage = await sqlHelper.GetExportCard(request.SpecialtyID, request.UserID, request.CardID, user.SelectedLocation);
                    break;
                default:
                    return ResponseHelper.CsvResponse("Unknown export type");
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

            return ResponseHelper.CsvResponse(dataExport.ToTable());
        }

        // GET api/values/5
        [SwaggerOperation("GetCaseOverview")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OverviewScreen))]
        [Route("caseOverview")]
        public async Task<HttpResponseMessage> GetCaseOverview(DateTime beginDate, DateTime endDate, int? specialtyId = null, int? bundleId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var result = await sqlHelper.GetCaseOverview(user.ProviderID, user.LocationID, beginDate, endDate, specialtyId, bundleId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("CleanupPatients")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("cleanupPatients")]
        public async Task<HttpResponseMessage> CleanupPatients()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var patients = await sqlHelper.GetCleanupPatients(user.ProviderID, user.LocationID);
            var result = await secureSqlHelper.CleanupPatients(patients);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
        
        [SwaggerOperation("PutImportFile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("importFile")]
        public async Task<HttpResponseMessage> PutImportFile(int importTypeId)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            int? logId = null;

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


                var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);
                logId = await sqlHelper.InsertImportLog(user.SelectedLocation, importTypeId, user.UserID, fileParser.Records.Count, fileName);

                if (fileParser.Records == null)
                    return Request.CreateResponse(HttpStatusCode.OK, fileParser.Status);

                var secureUser = await sqlHelper.GetUser(user.SelectedLocation, user.UserID);
                ImportResult result;

                if (importTypeId == 5 || importTypeId == 8)
                {
                    result = await sqlHelper.UpdateCardItemImport(fileParser.Records.Select(r => r as CardImport).ToList(),
                        fileParser.Relations, user.SelectedLocation);

                    foreach (var message in result.Messages)
                    {
                        await sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message, null, null);
                    }
                }
                else
                {
                    foreach (var record in fileParser.Records)
                    {
                        var secureId = await secureSqlHelper.InsertStagingData(record, user.UserID,
                            secureUser.FirstName, secureUser.LastName, (int)secureUser.RoleID);

                        result = await sqlHelper.InsertStagingData(user.SelectedLocation, secureId, record, fileParser.Relations, user.UserID);
                        foreach (var message in result.Messages)
                        {
                            await sqlHelper.InsertImportMessage(user.SelectedLocation, logId.Value, "WARN", message,
                                (record as ScheduleImport)?.MRN, (record as ScheduleImport)?.ScheduleDate);
                        }
                    }
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
        [SwaggerOperation("GetReports")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsSummary>))]
        [HttpGet]
        [Route("reports")]
        public async Task<HttpResponseMessage> GetReports()
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var sqlHelper = new SqlHelper();
            var locations = await sqlHelper.GetUserLocations(user.UserID, user.LocationID);
            var specialties = await sqlHelper.GetSpecialtyMaster();
            var surgeons = await sqlHelper.GetSurgeons(null, user.SelectedLocation);
            var procedures = await sqlHelper.GetProcedures(null, user.SelectedLocation);
            var trays = await sqlHelper.GetItems("TRAY", null, null, user.SelectedLocation);
            var cardCategories = await sqlHelper.GetCardCategories();
            var lookups = await sqlHelper.GetTrayInstrumentLookups(user.SelectedLocation);
            var items = await sqlHelper.GetItems(null, null, true, user.SelectedLocation);
            var roomGroups = await sqlHelper.GetRoomGroups(user.SelectedLocation);
            var cpts = await sqlHelper.GetKnownCPTCodes(user.ProviderID, user.LocationID);
            var proposedTrays = await sqlHelper.GetProposedTrays(null, user.SelectedLocation);
            var proposalPhases = await sqlHelper.GetTrayProposalPhases(user.SelectedLocation);
            var caseProfiles = await sqlHelper.GetCaseProfiles(user.SelectedLocation);

            var procedureProfiles = await sqlHelper.GetProcedureProfiles();
            var trayTypes = await sqlHelper.GetTrayTypes();
            var metrics = await sqlHelper.GetProcedureProfileMetrics(null, user.SelectedLocation);

            var cardCategoryXref = await sqlHelper.GetSpecialtyProcedureGroup(user.SelectedLocation);

            foreach (var specialty in specialties)
            {
                specialty.CardCategories = cardCategoryXref.Where(c => c.SpecialtyID == specialty.SpecialtyID).ToList();
            }

            return Request.CreateResponse(HttpStatusCode.OK, new
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
            var sqlHelper = new SqlHelper();
            var providers = await sqlHelper.GetOpFlowSetup();

            var provider = providers.FirstOrDefault(p => p.ProviderID == user.ProviderID);
            var location = provider?.Locations?.FirstOrDefault(l => l.LocationID == user.LocationID);

            return location.LocationName ?? "Unknown Location";
        }

        // GET api/values/5
        [SwaggerOperation("ProcedureMixReport")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("procedureMix")]
        [Route("procedureMix/{format}")]
        public async Task<HttpResponseMessage> ProcedureMixReport([FromBody] AdministrationReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);

            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            var analytics = await sqlHelper.GetAdministrationProcedureMix(post.SpecialtyId, post.TrayId, post.ItemId,
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
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<AnalyticsCountSummary>))]
        [HttpPut]
        [HttpPost]
        [Route("trayConcordance")]
        [Route("trayConcordance/{format}")]
        public async Task<HttpResponseMessage> TrayConcordanceReport([FromBody] AdministrationUsageReportPost post, string format = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            if (user.RoleType != "Internal" && user.RoleType != "Admin")
                return Request.CreateResponse(HttpStatusCode.NotFound);


            format = format ?? "IMAGE";

            var sqlHelper = new SqlHelper();
            if (post.SpecialtyID == null &&
                post.ProcedureID == null &&
                post.TrayID == null &&
                post.TrayTypeID == null &&
                post.CardCategoryID == null &&
                post.MetricID == null &&
                post.LocationId == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Error = true });
            }

            var analytics = await sqlHelper.GetAdministrationConcordanceReportData(post.SpecialtyID, post.ProcedureID, post.TrayID, post.TrayTypeID, post.VendorTray,
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
                return ResponseHelper.CsvResponse(concordance.ToTable());
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
                return ResponseHelper.PdfResponse(result);
            }
            else
            {
                var webImage = ImageHelper.CreateWebImage(result);

                return ResponseHelper.CompositeImageResponse(Request, summary, webImage);
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