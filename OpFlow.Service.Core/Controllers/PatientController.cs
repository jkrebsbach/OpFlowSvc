using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PatientController : OpFlowController
    {
        public PatientController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        public async Task<ActionResult> Get(int patientId)
        {
            var user = await GetUserSecurity();
            
            var userObject = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

            //var patient = await secureSqlHelper.GetPatient(patientId,
            //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);
            Patient patient = null;

            if (patient == null)
                return NotFound("Patient not found");

            return Ok(patient);
        }

        // GET api/values/5
        [Route("api/patient/array")]
        [HttpPost]
        public async Task<ActionResult> PatientArray([FromBody]PatientArrayPost post)
        {
            var patients = new List<Patient>();
            
            if (post?.PatientArray == null) return Ok(patients);

            var user = await GetUserSecurity();
            

            var userObject = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

            foreach (var patientId in post?.PatientArray)
            {
                //var patient = await secureSqlHelper.GetPatient(patientId,
                //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);
                Patient patient = null;

                if (patient != null)
                    patients.Add(patient);
            }

            return Ok(patients);
        }

        [Route("api/patient/array")]
        [HttpGet]
        public async Task<ActionResult> GetArray(string patientIdArrayJson)
        {
            var patientIds = new List<int>();
            var patients = new List<Patient>();

            if (patientIdArrayJson != null)
                patientIds = JsonSerializer.Deserialize<List<int>>(patientIdArrayJson);

            if (patientIds == null) return Ok(patients);
        
            var user = await GetUserSecurity();
            

            var userObject = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            foreach (var patientId in patientIds)
            {
                //var patient = await secureSqlHelper.GetPatient(patientId,
                //    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);
                Patient patient = null;

                if (patient != null)
                    patients.Add(patient);
            }

            return Ok(patients);
        }


    // POST api/values
        public async Task<ActionResult> Post([FromBody]PatientPost patient)
        {
            var secureUser = await GetUserSecurity();
            

            var user = await _sqlHelper.GetUser(secureUser.SelectedLocation,  secureUser.UserID);

            //var patientId = await secureSqlHelper.CreatePatient(patient.PatientAcctNbr,
            //    patient.BirthDate, patient.Gender, patient.FirstName, patient.LastName, patient.MiddleInitial, patient.BMI,
            //   secureUser.UserID, user.FirstName, user.LastName, (int)user.RoleID);
            var patientId = -1;

            return CreatedAtAction("Post", patientId);
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}