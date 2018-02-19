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
    [Authorize]
    public class MessagingController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetMessages")]
        [Route("api/message/list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Messaging>))]
        public HttpResponseMessage GetCaseMessaging(int providerId, int locationId, int? caseId = null, int? caseGroupId = null, int? recipientId = null)
        {
            var username = HttpContext.Current.User.Identity.Name;
            var userId = DataAccess.SqlHelper.GetUsers(username).FirstOrDefault()?.UserID ?? 0;

            var result = DataAccess.SqlHelper.GetMessaging(userId, caseId, caseGroupId, recipientId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetMessageGroups")]
        [Route("api/message/groups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        public HttpResponseMessage GetCaseMessageGroups(int providerId, int locationId)
        {
            var username = HttpContext.Current.User.Identity.Name;
            var userId = DataAccess.SqlHelper.GetUsers(username).FirstOrDefault()?.UserID ?? 0;

            var result = DataAccess.SqlHelper.GetMessageGroups(userId, providerId, locationId);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }
    }
}