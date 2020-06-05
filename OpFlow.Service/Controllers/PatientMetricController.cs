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
    [RoutePrefix("api/patientMetric")]
    public class PatientMetricController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<PatientMetric>))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Get()
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var metrics = await sqlHelper.GetPatientMetrics(user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, metrics);
        }

        // GET api/values/5
        [SwaggerOperation("Put")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(int))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<HttpResponseMessage> Put([FromBody] PatientMetricPost request)
        {
            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper();

            var metrics = await sqlHelper.InsertPatientMetric(request.TextPayload, user.SelectedLocation);

            return Request.CreateResponse(HttpStatusCode.OK, metrics);
        }
    }
}