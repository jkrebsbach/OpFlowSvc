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
        public async Task<HttpResponseMessage> Get(string metricType)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var metrics = await sqlHelper.GetProcedureProfileMetrics(metricType, user.LocationID);

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

            var result = await sqlHelper.InsertProcedureProfileMetric(request.MetricType, request.QuestionType, 
                request.TextPayload, request.ParentMetricAnswerID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("Put")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Put(int metricId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileMetric(metricId, 
                request.QuestionOwner, request.QuestionType, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("Delete")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Delete(int metricId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileMetric(metricId, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PostAnswer")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("answer")]
        [HttpPost]
        public async Task<HttpResponseMessage> PostAnswer(int metricId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.InsertProcedureProfileMetricAnswer(metricId, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutAnswer")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("answer")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutAnswer(int metricAnswerId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileMetricAnswer(metricAnswerId, request.TextPayload, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("DeleteAnswer")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("answer")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteAnswer(int metricAnswerId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileMetricAnswer(metricAnswerId, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("profile")]
        [HttpPut]
        public async Task<HttpResponseMessage> PutProfile(int procedureProfileId, int metricId, [FromBody] ProcedureProfileMetricXrefPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.UpdateProcedureProfileMetricXref(procedureProfileId, metricId, request.Answers, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("PutProfile")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        [Route("profile")]
        [HttpDelete]
        public async Task<HttpResponseMessage> DeleteProfile(int procedureProfileId, int metricId)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var result = await sqlHelper.DeleteProcedureProfileMetricXref(procedureProfileId, metricId, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}