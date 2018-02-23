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
    public class ImageController : OpFlowControllerBase
    {
        // GET api/surgery?surgeryId=5&caseId=1&providerId=1&bundleFlag=Y
        [SwaggerOperation("GetImage")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(byte[]))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> GetSurgery(int surgeryId, int cardId, int flowId)
        {
            var user = GetUserSecurity();

            var binary = await DataAccess.BlobStorageHelper.GetBlobBytes(user.ProviderID, surgeryId, cardId, flowId);
            
            return binary == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, binary);
        }


        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public async Task<IHttpActionResult> Put(int surgeryId, int cardId, int flowId, [FromBody]byte[] value)
        {
            if (value == null || value.Length == 0)
                return StatusCode(HttpStatusCode.Ambiguous);

            var user = GetUserSecurity();

            await DataAccess.BlobStorageHelper.PutBlobBytes(user.ProviderID, surgeryId, cardId, flowId, value);

            return Ok();
        }
    }
}