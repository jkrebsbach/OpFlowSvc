using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Data.Administration;
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
        public HttpResponseMessage GetImportTypes()
        {
            var user = CacheUtil.GetUserSecurity();

            var importTypes = SqlHelper.GetImportTypes(user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, importTypes);
        }

        // GET api/values/5
        [SwaggerOperation("GetImportDetails")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(ImportDetail))]
        [Route("api/administration/importDetails")]
        public HttpResponseMessage GetImportDetail(int importTypeId)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = new ImportDetail()
            {
                ImportDefinitions = SqlHelper.GetImportDefinition(importTypeId, user.ProviderID, user.LocationID),
                ImportLogs = SqlHelper.GetImportLog(importTypeId, user.ProviderID, user.LocationID)
            };

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        [SwaggerOperation("PutImportFile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [HttpPut]
        [Route("api/administration/importFile")]
        public async Task<HttpResponseMessage> PutImportFile(int importTypeId)
        {
            try
            {
                var user = CacheUtil.GetUserSecurity();

                var provider = new MultipartMemoryStreamProvider();
                await Request.Content.ReadAsMultipartAsync(provider);

                // extract file name and file contents
                var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                    .FirstOrDefault(p => p.Name.ToLower() == "filename");
                var fileName = fileNameParam?.Value.Trim('"') ?? "";
                var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

                var fileParser = new FileParser(fileName, fileContents);

                var records = fileParser.ParseFile(importTypeId);

                if (records != null)
                {
                    foreach (var record in records)
                    {

                        var secureId = await SecureSqlHelper.InsertStagingData(record, user.DatabaseName);
                        SqlHelper.InsertStagingData(user.ProviderID, user.LocationID, secureId, record);
                    }

                    SqlHelper.InsertImportLog(user.ProviderID, user.LocationID, importTypeId, user.UserID, records.Count, fileName);
                }

                return Request.CreateResponse(HttpStatusCode.OK, fileParser.Status);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

    }
}