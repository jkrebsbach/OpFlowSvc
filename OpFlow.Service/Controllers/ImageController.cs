using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
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

            var binary = await DataAccess.BlobStorageHelper.ListBlobs(user.ProviderID, cardId, flowId, stepId, roleId);

            return binary == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, binary);
        }

        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetImage(int cardId, int flowId, int stepId, int roleId, string fileName)
        {
            var user = CacheUtil.GetUserSecurity();

            var binary = await DataAccess.BlobStorageHelper.GetBlobBytes(user.ProviderID, cardId, flowId, stepId, roleId, fileName);
            
            return binary == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, binary);
        }


        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public async Task<IHttpActionResult> Put(int cardId, int flowId, int stepId, int roleId, string fileName, [FromBody]byte[] value)
        {
            if (value == null || value.Length == 0)
                return StatusCode(HttpStatusCode.Ambiguous);

            var user = CacheUtil.GetUserSecurity();

            await DataAccess.BlobStorageHelper.PutBlobBytes(user.ProviderID, cardId, flowId, stepId, roleId, fileName, value);

            return Ok();
        }



        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public async Task<IHttpActionResult> Delete(int cardId, int flowId, int stepId, int roleId, string filename)
        {
            var user = CacheUtil.GetUserSecurity();

            await DataAccess.BlobStorageHelper.DeleteBlob(user.ProviderID, cardId, flowId, stepId, roleId, filename);

            return Ok();
        }
    }
}