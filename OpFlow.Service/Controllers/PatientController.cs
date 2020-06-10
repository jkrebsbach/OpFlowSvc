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
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var userObject = await sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

            var patient = await secureSqlHelper.GetPatient(patientId,
                user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

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
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var userObject = await sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

            foreach (var patientId in post?.PatientArray)
            {
                var patient = await secureSqlHelper.GetPatient(patientId,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);
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
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(user.SecureDatabaseName);

            var userObject = await sqlHelper.GetUser(user.SelectedLocation, user.UserID);

            foreach (var patientId in patientIds)
            { 
                var patient = await secureSqlHelper.GetPatient(patientId,
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);
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
            var sqlHelper = new SqlHelper();
            var secureSqlHelper = new SecureSqlHelper(secureUser.SecureDatabaseName);

            var user = await sqlHelper.GetUser(secureUser.SelectedLocation,  secureUser.UserID);

            var patientId = await secureSqlHelper.CreatePatient(patient.PatientAcctNbr,
                patient.BirthDate, patient.Gender, patient.FirstName, patient.LastName, patient.MiddleInitial, patient.BMI,
                secureUser.UserID, user.FirstName, user.LastName, (int)user.RoleID);

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