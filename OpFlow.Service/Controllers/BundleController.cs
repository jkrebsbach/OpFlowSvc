using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class BundleController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardBundle>))]
        public async Task<HttpResponseMessage> GetBundles(int? specialtyId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var bundles = await DataAccess.SqlHelper.GetBundles(specialtyId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, bundles);
        }

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<BundleProcedure>))]
        [Route("api/bundle/procedures")]
        public async Task<HttpResponseMessage> GetBundleProcedures(int bundleId)
        {
            var user = await CacheUtil.GetUserSecurity();

            var procedures = await SqlHelper.GetBundleProcedures(bundleId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, procedures);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]BundlePost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.NewBundle(value.BundleDescription, value.SpecialtyID, value.Procedures, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Put(int id, [FromBody]BundlePost value)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.UpdateBundle(id, value.BundleDescription, value.SpecialtyID, value.Procedures, user.ProviderID, user.LocationID);

            return Ok();
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Delete(int id)
        {
            var user = await CacheUtil.GetUserSecurity();

            var result = await SqlHelper.DeleteBundle(id, user.ProviderID, user.LocationID);

            return Ok();
        }
    }
}