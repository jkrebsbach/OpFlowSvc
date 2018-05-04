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
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class ImageController : ApiController
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [AllowAnonymous]
        [SwaggerOperation("GetFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/flowImage", Name = "GetFlowImage")]
        public async Task<HttpResponseMessage> GetFlowImage(int flowId, int flowImageId)
        {
            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);
            var binary = await BlobStorageHelper.GetBlobBytes(folder, flowImageId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [AllowAnonymous]
        [SwaggerOperation("GetSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/surgeryImage", Name = "GetSurgeryImage")]
        public async Task<HttpResponseMessage> GetSurgeryImage(int surgeryId, int surgeryImageId)
        {
            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);
            var binary = await BlobStorageHelper.GetBlobBytes(folder, surgeryImageId.ToString());

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
            var binary = await BlobStorageHelper.GetBlobBytes(folder, patientPositionId.ToString());

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
            await BlobStorageHelper.PutBlobBytes(folder, patientPositionId.ToString(), fileContents);

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