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
    [RoutePrefix("api/specialty")]

    public class SpecialtyController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Specialty>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetSpecialties()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var specialties = await sqlHelper.GetSpecialties(user.SelectedLocation);
            var cardCategories = await sqlHelper.GetSpecialtyProcedureGroup(user.SelectedLocation);

            foreach (var specialty in specialties)
            {
                specialty.CardCategories = cardCategories.Where(c => c.SpecialtyID == specialty.SpecialtyID).ToList();
            }

            return specialties == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, specialties);
        }
        // GET api/values/5
        [SwaggerOperation("Get")]
        [Route("master")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Specialty>))]
        [HttpGet]
        public async Task<HttpResponseMessage> GetMasterSpecialties()
        {
            var sqlHelper = new SqlHelper();

            var specialties = await sqlHelper.GetSpecialtyMaster();
            
            return specialties == null ?
                Request.CreateResponse(HttpStatusCode.NotFound) :
                Request.CreateResponse(HttpStatusCode.OK, specialties);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        public async Task<HttpResponseMessage> PutSpecialty(int specialtyId, [FromBody]SpecialtyPost specialty)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.UpdateSpecialty(specialtyId, specialty.Name, specialty.Description, specialty.MasterSpecialtyID, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Insert")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSpecialty([FromBody]SpecialtyPost specialty)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var specialtyId = await sqlHelper.InsertSpecialty(specialty.Name, specialty.Description, specialty.MasterSpecialtyID, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteSpecialty(int specialtyId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteSpecialty(specialtyId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("PutProcedureGroup")]
        [Route("procedureGroup")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProcedureGroup(int specialtyId, int procedureGroupId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.InsertSpecialtyProcedureGroup(specialtyId, procedureGroupId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("DeleteProcedureGroup")]
        [Route("procedureGroup")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProcedureGroup(int specialtyId, int procedureGroupId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            await sqlHelper.DeleteSpecialtyProcedureGroup(specialtyId, procedureGroupId, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, specialtyId);
        }
    }
}