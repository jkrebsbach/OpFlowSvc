using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var result = new ImportDetail()
            {
                ImportDefinitions = await sqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID),
                ImportLogs = await sqlHelper.GetImportLog(importTypeId, user.ProviderID, user.LocationID)
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
            var result = await sqlHelper.GetImportMessages(importLogId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
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

            var sqlHelper = new SqlHelper(user.CaseDatabaseName);
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

                await fileParser.ParseFile(sqlHelper, importTypeId, user.ProviderID, user.LocationID);


                var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);
                logId = await sqlHelper.InsertImportLog(user.ProviderID, user.LocationID, importTypeId, user.UserID, fileParser.Records.Count, fileName);

                if (fileParser.Records != null)
                {
                    var secureUser = await sqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

                    foreach (var record in fileParser.Records)
                    {
                        var secureId = await secureSqlHelper.InsertStagingData(record, user.UserID, 
                            secureUser.FirstName, secureUser.LastName, (int)secureUser.RoleID);

                        var result = await sqlHelper.InsertStagingData(user.ProviderID, user.LocationID, secureId, record, fileParser.Relations);
                        foreach (var message in result.Messages)
                        {
                            await sqlHelper.InsertImportMessage(user.ProviderID, user.LocationID, logId.Value, "WARN", message, 
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
                    await sqlHelper.InsertImportMessage(user.ProviderID, user.LocationID, logId.Value, "ERROR", ex.Message, null, null);

                throw;
            }
        }

    }
}