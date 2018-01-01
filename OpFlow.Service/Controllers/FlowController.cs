using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    public class FlowController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetByCardIdUserId")]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage GetFlowByCard(int cardId, int userId)
        {
            var flow = DataAccess.SqlHelper.GetFlowByCardUser(cardId, userId);

            return Request.CreateResponse(HttpStatusCode.OK, flow);
        }
    }
}