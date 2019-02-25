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
        [SwaggerOperation("GetFlowImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/flowImage", Name = "GetFlowImage")]
        public async Task<HttpResponseMessage> GetFlowImage(int flowId, int flowImageId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var flow = await SqlHelper.GetFlow(flowId, user.ProviderID, user.LocationID);

            if (flow == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var binary = await storageHelper.GetBlobBytes(folder, flowImageId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetSurgeryImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/surgeryImage", Name = "GetSurgeryImage")]
        public async Task<HttpResponseMessage> GetSurgeryImage(int surgeryId, int surgeryImageId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var surgery = await SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);

            if (surgery == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var binary = await storageHelper.GetBlobBytes(folder, surgeryImageId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetPatientPositionImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/patientPosition", Name = "GetPatientPositionImage")]
        public async Task<HttpResponseMessage> GetPatientPositionImage(int patientPositionId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var folder = "PatientPosition";

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var binary = await storageHelper.GetBlobBytes(folder, patientPositionId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetRoomSetupImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(SecureImage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("api/image/roomSetup", Name = "GetRoomSetupImage")]
        public async Task<HttpResponseMessage> GetRoomSetupImage(int roomSetupId, int roomSetupImageId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var roomSetup = await SqlHelper.GetRoomSetup(roomSetupId, user.ProviderID, user.LocationID);

            if (roomSetup == null)
                return Request.CreateResponse(HttpStatusCode.NotFound);


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);

            var storageHelper = BlobStorageHelper.GetHelper(user);

            var binary = await storageHelper.GetBlobBytes(folder, roomSetupImageId.ToString());

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                ImageResponse(binary);
        }


        // PUT api/values/5
        //[SwaggerOperation("UpdatePatientPositionImage")]
        //[SwaggerResponse(HttpStatusCode.OK)]
        //[HttpPost]
        //[Route("api/image/patientPosition", Name = "UpdatePatientPositionImage")]
        //public async Task<IHttpActionResult> PutPatientPositionImage(int patientPositionId)
        //{
        //    var provider = new MultipartMemoryStreamProvider();
        //    await Request.Content.ReadAsMultipartAsync(provider);

        //    // extract file name and file contents
        //    var fileNameParam = provider.Contents[0].Headers.ContentDisposition.Parameters
        //        .FirstOrDefault(p => p.Name.ToLower() == "filename");
        //    var fileName = fileNameParam?.Value.Trim('"') ?? "";
        //    var fileContents = await provider.Contents[0].ReadAsByteArrayAsync();

        //    var folder = "PatientPosition";
        //    await BlobStorageHelper.PutBlobBytes(folder, patientPositionId.ToString(), fileContents);

        //    return Ok();
        //}

        private HttpResponseMessage ImageResponse(byte[] payloadBytes)
        {
            var result = Request.CreateResponse(HttpStatusCode.OK,
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(payloadBytes)
                });

            return result;
        }


    }
}