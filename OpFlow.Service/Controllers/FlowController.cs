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
        [SwaggerOperation("GetByCardIdUserId")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Flow))]
        public HttpResponseMessage GetFlowByCard(int cardId, int userId)
        {
            var flow = DataAccess.SqlHelper.GetFlowByCardUser(cardId, userId);

            var result = new List<Flow>();
            if (flow != null)
                result.Add(flow);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetFlowTimings")]
        [Route("api/flow/timings")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<FlowStep>))]
        public HttpResponseMessage GetFlowTimings(int flowId, int surgeryId, int providerId, int locationId)
        {
            var result = DataAccess.SqlHelper.GetFlowTimings(flowId, surgeryId, providerId, locationId);

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
    }
}