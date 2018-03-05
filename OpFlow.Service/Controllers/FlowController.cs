using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class FlowController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Flow))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetFlow(int flowId, int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var flow = DataAccess.SqlHelper.GetFlow(flowId, cardId, user.ProviderID, user.LocationID);

            return flow == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) : 
                Request.CreateResponse(HttpStatusCode.OK, flow);
        }

        // GET api/values/5
        [SwaggerOperation("GetCardFlowList")]
        [Route("api/flow/cardFlowList")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Flow>))]
        public HttpResponseMessage GetCardFlowList(int cardId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var flowList = DataAccess.SqlHelper.GetCardFlowList(cardId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, flowList);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowInstructions")]
        [Route("api/flow/instructions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public HttpResponseMessage GetFlowInstructions(int flowId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowInstructions(flowId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowTimings")]
        [Route("api/flow/timings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowTiming>))]
        public HttpResponseMessage GetFlowTimings(int flowId, int? providerId = null, int? locationId = null, int? surgeryId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowTimings(flowId, user.ProviderID, user.LocationID, surgeryId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowComments")]
        [Route("api/flow/comments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public HttpResponseMessage GetFlowComments(int flowId, int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowComments(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowMessaging")]
        [Route("api/flow/messaging")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowMessaging>))]
        public HttpResponseMessage GetFlowMessaging(int flowId, int stepId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowMessaging(flowId, user.ProviderID, user.LocationID, stepId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowContent")]
        [Route("api/flow/content")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowContent>))]
        public HttpResponseMessage GetFlowContent(int flowId, int surgeryId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowContent(flowId, surgeryId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowNotifications")]
        [Route("api/flow/notifications")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowNotification>))]
        public HttpResponseMessage GetFlowNotifications(int flowId, int stepId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowNotifications(flowId, stepId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowFeedback")]
        [Route("api/flow/feedback")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowFeedback>))]
        public HttpResponseMessage GetFlowFeedback(int flowId, int stepId, int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetFlowFeedback(flowId, stepId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}