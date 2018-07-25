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
            var user = CacheUtil.GetUserSecurity();
            var userObject = SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);

            var patient = await DataAccess.SecureSqlHelper.GetPatient(patientId,
                user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID, 
                user.DatabaseName);

            if (patient == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Patient not found");

            return Request.CreateResponse(HttpStatusCode.OK, patient);
        }

        // GET api/values/5
        [SwaggerOperation("GetByList")]
        [Route("api/patient/array")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Patient>))]
        public async Task<HttpResponseMessage> GetArray(string patientIdArrayJson)
        {
            var patientIds = new List<int>();
            var patients = new List<Patient>();
            
            if (patientIdArrayJson != null)
                patientIds = JsonConvert.DeserializeObject<List<int>>(patientIdArrayJson);

            if (patientIds != null)
            {
                var user = CacheUtil.GetUserSecurity();
                var userObject = SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);

                foreach (var patientId in patientIds)
                {
                    var patient = await DataAccess.SecureSqlHelper.GetPatient(patientId,
                        user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID, 
                        user.DatabaseName);
                    if (patient != null)
                        patients.Add(patient);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, patients);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<HttpResponseMessage> Post([FromBody]PatientPost patient)
        {
            var secureUser = CacheUtil.GetUserSecurity();
            var user = SqlHelper.GetUser(secureUser.ProviderID, secureUser.LocationID, null, secureUser.UserID);

            var patientId = await SecureSqlHelper.CreatePatient(patient.PatientAcctNbr,
                patient.BirthDate, patient.Gender, patient.FirstName, patient.LastName, patient.MiddleInitial, patient.BMI,
                secureUser.UserID, user.FirstName, user.LastName, (int)user.RoleID,
                    secureUser.DatabaseName);

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