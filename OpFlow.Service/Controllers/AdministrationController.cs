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
    public class AdministrationController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("GetImportType")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportType>))]
        [Route("api/administration/importTypes")]
        public async Task<HttpResponseMessage> GetImportTypes()
        {
            var user = await CacheUtil.GetUserSecurity();

            var importTypes = await SqlHelper.GetImportTypes(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, importTypes);
        }

        // GET api/values/5
        [SwaggerOperation("GetImportDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ImportDetail))]
        [Route("api/administration/importDetails")]
        public async Task<HttpResponseMessage> GetImportDetail(int importTypeId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = new ImportDetail()
            {
                ImportDefinitions = await SqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID),
                ImportLogs = await SqlHelper.GetImportLog(importTypeId, user.ProviderID, user.LocationID)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetImportMessages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ImportMessage>))]
        [Route("api/administration/importMessages")]
        public async Task<HttpResponseMessage> GetImportMessages(int importLogId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetImportMessages(importLogId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetCaseOverview")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(OverviewScreen))]
        [Route("api/administration/caseOverview")]
        public async Task<HttpResponseMessage> GetCaseOverview(DateTime beginDate, DateTime endDate, int? specialtyId = null, int? bundleId = null)
        {
            var user = await CacheUtil.GetUserSecurity();
            
            var result = await SqlHelper.GetCaseOverview(user.ProviderID, user.LocationID, beginDate, endDate, specialtyId, bundleId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("PutImportFile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("api/administration/importFile")]
        public async Task<HttpResponseMessage> PutImportFile(int importTypeId)
        {
            var user = await CacheUtil.GetUserSecurity();
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

                await fileParser.ParseFile(importTypeId, user.ProviderID, user.LocationID);


                logId = await SqlHelper.InsertImportLog(user.ProviderID, user.LocationID, importTypeId, user.UserID, fileParser.Records.Count, fileName);

                if (fileParser.Records != null)
                {
                    var secureUser = await SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

                    foreach (var record in fileParser.Records)
                    {
                        var secureId = await SecureSqlHelper.InsertStagingData(record, user.UserID, 
                            secureUser.FirstName, secureUser.LastName, (int)secureUser.RoleID,
                            user.DatabaseName);

                        var result = await SqlHelper.InsertStagingData(user.ProviderID, user.LocationID, secureId, record, fileParser.Relations);
                        foreach (var message in result.Messages)
                        {
                            await SqlHelper.InsertImportMessage(user.ProviderID, user.LocationID, logId.Value, "WARN", message, (record as ScheduleImport)?.MRN);
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, fileParser.Status);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                RaygunClient client = new RaygunClient("f12C1dpwvycqBLOm2YT5rw==");
                client.Send(ex);

                if (logId != null)
                    await SqlHelper.InsertImportMessage(user.ProviderID, user.LocationID, logId.Value, "ERROR", ex.Message, null);

                throw;
            }
        }

    }
}