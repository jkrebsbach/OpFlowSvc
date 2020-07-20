using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    [RoutePrefix("api/procedureProfileMetric")]
    public class ProcedureProfileMetricController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<ProcedureProfileMetric>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Get()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var metrics = await sqlHelper.GetProcedureProfileMetrics(user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, metrics);
        }

        // GET api/values/5
        [SwaggerOperation("Post")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Post([FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileMetric(request.MetricType, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("Put")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Put(int patientMetricId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileMetric(patientMetricId, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Delete(int procedureProfileMetricId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileMetric(procedureProfileMetricId, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostAnswer")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("answer")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostAnswer(int procedureProfileMetricId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileMetricAnswer(procedureProfileMetricId, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutAnswer")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("answer")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutAnswer(int procedureProfileMetricAnswerId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileMetricAnswer(procedureProfileMetricAnswerId, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteAnswer")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("answer")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAnswer(int procedureProfileMetricAnswerId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileMetricAnswer(procedureProfileMetricAnswerId, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("profile")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProfile(int procedureProfileId, int procedureProfileMetricId, [FromBody] ProcedureProfileMetricXrefPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileMetricXref(procedureProfileId, procedureProfileMetricId, request.Answers, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("profile")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProfile(int procedureProfileId, int procedureProfileMetricId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileMetricXref(procedureProfileId, procedureProfileMetricId, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}