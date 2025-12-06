using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/procedureProfileMetric")]
    public class ProcedureProfileMetricController : OpFlowController
    {
        public ProcedureProfileMetricController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        public async Task<ActionResult> Get(string metricType)
        {
            var user = await GetUserSecurity();
            

            var metrics = await _sqlHelper.GetProcedureProfileMetrics(metricType, user.SelectedLocation);

            return Ok(metrics);
        }

        // GET api/values/5
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] ProcedureProfileMetricPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertProcedureProfileMetric(request.MetricType, request.QuestionType, 
                request.TextPayload, request.ParentMetricAnswerID, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [HttpPut]
        public async Task<ActionResult> Put(int metricId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateProcedureProfileMetric(metricId, 
                request.QuestionOwner, request.QuestionType, request.TextPayload, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [HttpDelete]
        public async Task<ActionResult> Delete(int metricId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteProcedureProfileMetric(metricId, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("answer")]
        [HttpPost]
        public async Task<ActionResult> PostAnswer(int metricId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.InsertProcedureProfileMetricAnswer(metricId, request.TextPayload, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("answer")]
        [HttpPut]
        public async Task<ActionResult> PutAnswer(int metricAnswerId, [FromBody] ProcedureProfileMetricPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateProcedureProfileMetricAnswer(metricAnswerId, request.TextPayload, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("answer")]
        [HttpDelete]
        public async Task<ActionResult> DeleteAnswer(int metricAnswerId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteProcedureProfileMetricAnswer(metricAnswerId, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("profile")]
        [HttpPut]
        public async Task<ActionResult> PutProfile(int procedureProfileId, int metricId, [FromBody] ProcedureProfileMetricXrefPost request)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.UpdateProcedureProfileMetricXref(procedureProfileId, metricId, request.Answers, user.LocationID);

            return Ok(result);
        }

        // GET api/values/5
        [Route("profile")]
        [HttpDelete]
        public async Task<ActionResult> DeleteProfile(int procedureProfileId, int metricId)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.DeleteProcedureProfileMetricXref(procedureProfileId, metricId, user.LocationID);

            return Ok(result);
        }
    }
}