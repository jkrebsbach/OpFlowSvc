using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class BundleController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<CardBundle>))]
        public HttpResponseMessage GetBundles(int specialtyId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var bundles = DataAccess.SqlHelper.GetBundles(specialtyId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, bundles);
        }

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<BundleProcedure>))]
        [Route("api/bundle/procedures")]
        public HttpResponseMessage GetBundleProcedures(int bundleId)
        {
            var user = CacheUtil.GetUserSecurity();

            var procedures = DataAccess.SqlHelper.GetBundleProcedures(bundleId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, procedures);
        }
    }
}