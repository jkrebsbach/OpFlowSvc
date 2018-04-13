using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.Models;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class ImageController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetImages")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<string>))]
        [Route("api/image/listImages")]
        public async Task<HttpResponseMessage> GetImages(int cardId, int flowId, int stepId, int roleId)
        {
            var user = CacheUtil.GetUserSecurity();

            var folder = DataAccess.BlobStorageHelper.Folder(user.ProviderID, cardId, flowId, stepId, roleId);
            var fileList = await DataAccess.BlobStorageHelper.ListBlobs(folder);

            return fileList == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, fileList);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetImage(int cardId, int flowId, int stepId, int roleId, string fileName)
        {
            var user = CacheUtil.GetUserSecurity();

            var folder = DataAccess.BlobStorageHelper.Folder(user.ProviderID, cardId, flowId, stepId, roleId);
            var binary = await DataAccess.BlobStorageHelper.GetBlobBytes(folder, fileName);
            
            return binary == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [AllowAnonymous]
        [SwaggerOperation("GetFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/flowImage", Name = "GetFlowImage")]
        public async Task<HttpResponseMessage> GetFlowImage(int flowId, int flowImageId)
        {
            var folder = DataAccess.BlobStorageHelper.Folder(flowId, 0, 0, 0, 0);
            var binary = await DataAccess.BlobStorageHelper.GetBlobBytes(folder, flowImageId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [AllowAnonymous]
        [SwaggerOperation("GetPatientPositionImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/patientPosition", Name = "GetPatientPositionImage")]
        public async Task<HttpResponseMessage> GetPatientPositionImage(int patientPositionId)
        {
            var folder = "PatientPosition";
            var binary = await DataAccess.BlobStorageHelper.GetBlobBytes(folder, patientPositionId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }


        // PUT api/values/5
        [SwaggerOperation("UpdatePatientPositionImage")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        [Route("api/image/patientPosition", Name = "UpdatePatientPositionImage")]
        public async Task<IHttpActionResult> PutPatientPositionImage(int patientPositionId)
        {
            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // extract file name and file contents
            var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                .FirstOrDefault(p => p.Name.ToLower() == "filename");
            var fileName = fileNameParam?.Value.Trim('"') ?? "";
            var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

            var folder = "PatientPosition";
            await DataAccess.BlobStorageHelper.PutBlobBytes(folder, patientPositionId.ToString(), fileContents);

            return Ok();
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public async Task<IHttpActionResult> Put(int cardId, int flowId, int stepId, int roleId)
        {
            var user = CacheUtil.GetUserSecurity();

            var provider = new MultipartMemoryStreamProvider();
            await Request.Content.ReadAsMultipartAsync(provider);

            // extract file name and file contents
            var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
                .FirstOrDefault(p => p.Name.ToLower() == "filename");
            var fileName = fileNameParam?.Value.Trim('"') ?? "";
            var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

            var folder = DataAccess.BlobStorageHelper.Folder(user.ProviderID, cardId, flowId, stepId, roleId);
            await DataAccess.BlobStorageHelper.PutBlobBytes(folder, fileName, fileContents);

            return Ok();
        }



        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<IHttpActionResult> Delete(int cardId, int flowId, int stepId, int roleId, string filename)
        {
            var user = CacheUtil.GetUserSecurity();

            var folder = DataAccess.BlobStorageHelper.Folder(user.ProviderID, cardId, flowId, stepId, roleId);
            await DataAccess.BlobStorageHelper.DeleteBlob(folder, filename);

            return Ok();
        }

        private HttpResponseMessage ImageResponse(byte[] payloadBytes)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(payloadBytes)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return result;
        }


    }
}