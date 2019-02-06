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

    public class SpecialtyController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("Get")]
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Specialty>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetSpecialties(int? providerId = null, int? locationId = null)
        {
            var user = await CacheUtil.GetUserSecurity();

            var specialties = await DataAccess.SqlHelper.GetSpecialties(user.ProviderID, user.LocationID);

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
        public async Task<HttpResponseMessage> PutSpecialty(int specialtyId, [FromBody]SpecialtyPost specialty)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.UpdateSpecialty(specialtyId, specialty.Name, specialty.Description, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Insert")]
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSpecialty([FromBody]SpecialtyPost specialty)
        {
            var user = await CacheUtil.GetUserSecurity();

            var specialtyId = await DataAccess.SqlHelper.InsertSpecialty(specialty.Name, specialty.Description, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Delete")]
        [Route("api/specialty")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteSpecialty(int specialtyId)
        {
            var user = await CacheUtil.GetUserSecurity();

            await DataAccess.SqlHelper.DeleteSpecialty(specialtyId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }
    }
}