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
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Specialty>))]
        [HttpGet]
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
        [HttpPut]
        public HttpResponseMessage PutSpecialty(int specialtyId, [FromBody]SpecialtyPost specialty)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.UpdateSpecialty(specialtyId, specialty.Name, specialty.Description, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Insert")]
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public HttpResponseMessage PostSpecialty([FromBody]SpecialtyPost specialty)
        {
            var user = CacheUtil.GetUserSecurity();

            var specialtyId = DataAccess.SqlHelper.InsertSpecialty(specialty.Name, specialty.Description, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Delete")]
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public HttpResponseMessage DeleteSpecialty(int specialtyId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeleteSpecialty(specialtyId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }
    }
}