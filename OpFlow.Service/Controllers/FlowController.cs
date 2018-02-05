using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using OpFlow.Data;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    public class FlowController : ApiController
    {
        // GET api/values/5
        [SwaggerOperation("Get")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Flow))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public HttpResponseMessage GetFlow(int flowId, int cardId, int providerId, int locationId)
        {
            var flow = DataAccess.SqlHelper.GetFlow(flowId, cardId, providerId, locationId);

            return flow == null ? 
                Request.CreateResponse(HttpStatusCode.NotFound) : 
                Request.CreateResponse(HttpStatusCode.OK, flow);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowInstructions")]
        [Route("api/flow/instructions")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public HttpResponseMessage GetFlowInstructions(int flowId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetFlowInstructions(flowId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowTimings")]
        [Route("api/flow/timings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowTiming>))]
        public HttpResponseMessage GetFlowTimings(int flowId, int providerId, int locationId, int? surgeryId = null)
        {
            var result = DataAccess.SqlHelper.GetFlowTimings(flowId, providerId, locationId, surgeryId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowComments")]
        [Route("api/flow/comments")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public HttpResponseMessage GetFlowComments(int flowId, int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetFlowComments(flowId, surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowMessaging")]
        [Route("api/flow/messaging")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowMessaging>))]
        public HttpResponseMessage GetFlowMessaging(int flowId, int providerId, int locationId, int stepId)
        {
            var result = DataAccess.SqlHelper.GetFlowMessaging(flowId, providerId, locationId, stepId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowContent")]
        [Route("api/flow/content")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowContent>))]
        public HttpResponseMessage GetFlowContent(int flowId, int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetFlowContent(flowId, surgeryId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowNotifications")]
        [Route("api/flow/notifications")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowNotification>))]
        public HttpResponseMessage GetFlowNotifications(int flowId, int stepId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetFlowNotifications(flowId, stepId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowFeedback")]
        [Route("api/flow/feedback")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowNotification>))]
        public HttpResponseMessage GetFlowFeedback(int flowId, int stepId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetFlowFeedback(flowId, stepId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}