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
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class PatientController : OpFlowControllerBase
    {
        // GET api/values/5
        [SwaggerOperation("GetById")]
        [SwaggerResponse(HttpStatusCode.OK, Type=typeof(Patient))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage Get(int patientId)
        {
            var user = GetUserSecurity();

            var patient = DataAccess.SecureSqlHelper.GetPatient(patientId, user.DatabaseName);

            if (patient == null)
                return Request.CreateResponse(HttpStatusCode.NotFound, "Patient not found");

            return Request.CreateResponse(HttpStatusCode.OK, patient);
        }

        // GET api/values/5
        [SwaggerOperation("GetByList")]
        [Route("api/patient/array")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IEnumerable<Patient>))]
        public HttpResponseMessage GetArray(string patientIdArrayJson)
        {
            var user = GetUserSecurity();

            var patientIds = new List<int>();
            var patients = new List<Patient>();
            
            if (patientIdArrayJson != null)
                patientIds = JsonConvert.DeserializeObject<List<int>>(patientIdArrayJson);

            if (patientIds != null)
            {
                foreach (var patientId in patientIds)
                {
                    var patient = DataAccess.SecureSqlHelper.GetPatient(patientId, user.DatabaseName);
                    if (patient != null)
                        patients.Add(patient);
                }
            }

            return Request.CreateResponse(HttpStatusCode.OK, patients);
        }

        // POST api/values
        [SwaggerOperation("Create")]
        [SwaggerResponse(HttpStatusCode.Created)]
        public async Task<IHttpActionResult> Post([FromBody]PatientPost patient)
        {
            DataAccess.SqlHelper.CreatePatient(patient);

            return Ok();
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