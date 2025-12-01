using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/image")]
    public class ImageController : OpFlowController
    {
        private BlobStorageHelper _blobStorageHelper;

        public ImageController(BlobStorageHelper blobStorageHelper,
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("flowImage", Name = "GetFlowImage")]
        public async Task<ActionResult> GetFlowImage(int flowId, int flowImageId)
        {
            var user = await GetUserSecurity();
            

            var flow = await _sqlHelper.GetFlow(flowId, user.SelectedLocation);

            if (flow == null)
                return NotFound();

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.FlowImages, flowId);


            var binary = await _blobStorageHelper.GetBlobBytes(folder, flowImageId.ToString());

            return binary == null ?
                NotFound() :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("surgeryImage", Name = "GetSurgeryImage")]
        public async Task<ActionResult> GetSurgeryImage(int surgeryId, int surgeryImageId)
        {
            var user = await GetUserSecurity();
            

            var surgery = await _sqlHelper.GetSurgery(surgeryId, user.SelectedLocation);

            if (surgery == null)
                return NotFound();

            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.SurgeryImages, surgeryId);


            var binary = await _blobStorageHelper.GetBlobBytes(folder, surgeryImageId.ToString());

            return binary == null ?
                NotFound() :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("patientPosition", Name = "GetPatientPositionImage")]
        public async Task<ActionResult> GetPatientPositionImage(int patientPositionId)
        {
            var user = await GetUserSecurity();
            var folder = "PatientPosition";


            var binary = await _blobStorageHelper.GetBlobBytes(folder, patientPositionId.ToString());

            return binary == null ?
                NotFound() :
                ImageResponse(binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [Route("roomSetup", Name = "GetRoomSetupImage")]
        public async Task<ActionResult> GetRoomSetupImage(int roomSetupId, int roomSetupImageId)
        {
            var user = await GetUserSecurity();
            

            var roomSetup = await _sqlHelper.GetRoomSetup(roomSetupId, user.SelectedLocation);

            if (roomSetup == null)
                return NotFound();


            var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.RoomSetupImages, roomSetupId);


            var binary = await _blobStorageHelper.GetBlobBytes(folder, roomSetupImageId.ToString());

            return binary == null ?
                NotFound() :
                ImageResponse(binary);
        }

        [Route("trayPhoto/{trayProposalId}")]
        [HttpGet]
        public async Task<ActionResult> GetTrayPhotoBytes(int trayProposalId)
        {
            var user = await GetUserSecurity();


            var trayProposals = await _sqlHelper.GetProposedTrays(trayProposalId, user.SelectedLocation);
            var trayProposal = trayProposals.FirstOrDefault();
            if (trayProposal != null)
            {
                var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.TrayProposalImages, trayProposalId);


                var binary = await _blobStorageHelper.GetBlobBytes(folder, trayProposalId.ToString());

                return binary == null ?
                    NotFound() :
                    ImageResponse(binary);
            }


            return Ok((int?)null);
        }

        [HttpGet]
        [Route("orgChart", Name = "GetOrgChart")]
        public async Task<ActionResult> GetOrgChart(string type, int id)
        {
            var user = await GetUserSecurity();

            

            try
            {

                string filename = null;
                byte[] binary;

                if (type == "T")
                {
                    var proposals = await _sqlHelper.GetProposedTrays(id, user.SelectedLocation);
                    var proposal = proposals.First();

                    filename = proposal.OrgChart;

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, id);
                    binary = await _blobStorageHelper.GetBlobBytes(folder, id.ToString());
                }
                else
                {
                    var attachments = await _sqlHelper.GetOrgChartAttachments(user.SelectedLocation);
                    var attachment = attachments.First(a => a.OrgChartID == id);

                    filename = attachment.Filename;

                    var folder = BlobStorageHelper.Folder(BlobStorageHelper.ImageType.OrgChartImages, -1);
                    binary = await _blobStorageHelper.GetBlobBytes(folder, id.ToString());
                }

                return binary == null ?
                    NotFound() :
                    ImageResponse(binary);
            }
            catch (Exception ex)
            {
                LogHelper.LogException(ex);
                throw;
            }
        }


        // PUT api/values/5
        //[SwaggerOperation("UpdatePatientPositionImage")]
        //[SwaggerResponse(HttpStatusCode.OK)]
        //[HttpPost]
        //[Route("patientPosition", Name = "UpdatePatientPositionImage")]
        //public async Task<ActionResult> PutPatientPositionImage(int patientPositionId)
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

        private ActionResult ImageResponse(byte[] payloadBytes)
        {
            var result = Ok(
                new SecureImage()
                {
                    DocumentBytes = "data:image/png;base64, " + Convert.ToBase64String(payloadBytes)
                });

            return result;
        }


    }
}