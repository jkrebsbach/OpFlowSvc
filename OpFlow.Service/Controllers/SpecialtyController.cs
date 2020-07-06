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
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var masterSpecialties = await sqlHelper.GetSpecialtyMaster();
            var specialties = await sqlHelper.GetSpecialties(user.SelectedLocation);
            var cardCategories = await sqlHelper.GetSpecialtyProcedureGroup(user.SelectedLocation);

            foreach (var specialty in specialties)
            {
                specialty.CardCategories = cardCategories.Where(c => c.SpecialtyID == specialty.SpecialtyID).ToList();
            }

            foreach (var masterSpecialty in masterSpecialties)
            {
                masterSpecialty.SpecialtyList = specialties.Where(s => s.MasterSpecialtyID == masterSpecialty.SpecialtyID).ToList();
                masterSpecialty.CardCategories = masterSpecialty.SpecialtyList.SelectMany(s => s.CardCategories).ToList();
            }
            
            return Request.CreateResponse(HttpStatusCode.OK, masterSpecialties);
        }

        // PUT api/roomSetup/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostSpecialty([FromBody]SpecialtyPost specialty)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateSpecialty(specialty.Specialties, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, result);
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