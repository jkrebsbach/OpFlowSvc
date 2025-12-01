using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using OpFlow.Data;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/specialty")]

    public class SpecialtyController : OpFlowController
    {
        public SpecialtyController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        [HttpGet]
        public async Task<ActionResult> GetSpecialties()
        {
            var user = await GetUserSecurity();
            

            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var cardCategories = await _sqlHelper.GetSpecialtyProcedureGroup(user.SelectedLocation);

            foreach (var specialty in specialties)
            {
                specialty.CardCategories = cardCategories.Where(c => c.SpecialtyID == specialty.SpecialtyID).ToList();
            }

            return specialties == null ?
                NotFound() :
                Ok(specialties);
        }
        // GET api/values/5
        [Route("master")]
        [HttpGet]
        public async Task<ActionResult> GetMasterSpecialties()
        {
            var user = await GetUserSecurity();
            

            var masterSpecialties = await _sqlHelper.GetSpecialtyMaster();
            var specialties = await _sqlHelper.GetSpecialties(user.SelectedLocation);
            var cardCategories = await _sqlHelper.GetSpecialtyProcedureGroup(user.SelectedLocation);

            foreach (var specialty in specialties)
            {
                specialty.CardCategories = cardCategories.Where(c => c.SpecialtyID == specialty.SpecialtyID).ToList();
            }

            foreach (var masterSpecialty in masterSpecialties)
            {
                masterSpecialty.SpecialtyList = specialties.Where(s => s.MasterSpecialtyID == masterSpecialty.SpecialtyID).ToList();
                masterSpecialty.CardCategories = masterSpecialty.SpecialtyList.SelectMany(s => s.CardCategories).ToList();
            }
            
            return Ok(masterSpecialties);
        }

        // PUT api/roomSetup/values/5
        [HttpPost]
        public async Task<ActionResult> PostSpecialty([FromBody]SpecialtyPost specialty)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateSpecialty(specialty.Specialties, user.SelectedLocation);

            return Ok(result);
        }

        // PUT api/roomSetup/values/5
        [Route("procedureGroup")]
        [HttpPut]
        public async Task<ActionResult> PutProcedureGroup(int specialtyId, int procedureGroupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.InsertSpecialtyProcedureGroup(specialtyId, procedureGroupId, user.SelectedLocation);

            return Ok(specialtyId);
        }

        // PUT api/roomSetup/values/5
        [Route("procedureGroup")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProcedureGroup(int specialtyId, int procedureGroupId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeleteSpecialtyProcedureGroup(specialtyId, procedureGroupId, user.SelectedLocation);

            return Ok(specialtyId);
        }
    }
}