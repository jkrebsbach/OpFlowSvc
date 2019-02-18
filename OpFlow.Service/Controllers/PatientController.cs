using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Newtonsoft.Json;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class PatientController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(Patient))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Get(int patientId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            var patient = await SecureSqlHelper.GetPatient(patientId,
                user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID, 
                user.SecureDatabaseName);

            if (patient == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Patient not found");

            return Request.CreateResponse(HttpStatusCode.OK, patient);
        }

        // GET api/values/5
        [SwaggerOperation("PostPatientArrayByList")]
        [Route("api/patient/array")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Patient>))]
        public async Task<HttpResponseMessage> PatientArray([FromBody]PatientArrayPost post)
        {
            var patients = new List<Patient>();
            
            if (post?.PatientArray == null) return Request.CreateResponse(HttpStatusCode.OK, patients);

            var user = await CacheUtil.GetUserSecurity();
            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            foreach (var patientId in post?.PatientArray)
            {
                var patient = await SecureSqlHelper.GetPatient(patientId,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID, 
                    user.SecureDatabaseName);
                if (patient != null)
                    patients.Add(patient);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patients);
        }

        [SwaggerOperation("GetByList")]
        [Route("api/patient/array")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Patient>))]
        public async Task<HttpResponseMessage> GetArray(string patientIdArrayJson)
        {
            var patientIds = new List<int>();
            var patients = new List<Patient>();

            if (patientIdArrayJson != null)
                patientIds = JsonConvert.DeserializeObject<List<int>>(patientIdArrayJson);

            if (patientIds == null) return Request.CreateResponse(HttpStatusCode.OK, patients);
        
            var user = await CacheUtil.GetUserSecurity();
            var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

            foreach (var patientId in patientIds)
            { 
                var patient = await SecureSqlHelper.GetPatient(patientId,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID,
                    user.SecureDatabaseName);
                if (patient != null)
                    patients.Add(patient);
            }

            return Request.CreateResponse(HttpStatusCode.OK, patients);
        }


    // POST api/values
    [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]PatientPost patient)
        {
            var secureUser = await CacheUtil.GetUserSecurity();
            var user = await SqlHelper.GetUser(secureUser.ProviderID, secureUser.LocationID,  secureUser.UserID);

            var patientId = await SecureSqlHelper.CreatePatient(patient.PatientAcctNbr,
                patient.BirthDate, patient.Gender, patient.FirstName, patient.LastName, patient.MiddleInitial, patient.BMI,
                secureUser.UserID, user.FirstName, user.LastName, (int)user.RoleID,
                    secureUser.SecureDatabaseName);

            return Request.CreateResponse(HttpStatusCode.Created, patientId);
        }

        // PUT api/values/5
        [SwaggerOperation("Update")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public void Delete(int id)
        {
        }
    }
}