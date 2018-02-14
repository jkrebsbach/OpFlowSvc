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
        public HttpResponseMessage GetBundles(int specialtyId, int providerId, int locationId)
        {
            var bundles = DataAccess.SqlHelper.GetBundles(specialtyId, providerId, locationId);

            return bundles == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, bundles);
        }
    }
}