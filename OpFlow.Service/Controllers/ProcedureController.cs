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
    public class ProcedureController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Procedure>))]
        public HttpResponseMessage GetProcedures(int specialtyId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();
            
            var procedures = DataAccess.SqlHelper.GetProcedures(specialtyId, user.ProviderID, user.LocationID);

            return procedures == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, procedures);
        }

    }
}