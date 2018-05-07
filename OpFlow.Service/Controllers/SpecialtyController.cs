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

    public class SpecialtyController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Specialty>))]
        public HttpResponseMessage GetSpecialties(int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var specialties = DataAccess.SqlHelper.GetSpecialties(user.ProviderID, user.LocationID);

            return specialties == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, specialties);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Update")]
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage PutSpecialty(int specialtyId, [FromBody]SpecialtyPost specialty)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateSpecialty(specialtyId, specialty.Description, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }
    }
}