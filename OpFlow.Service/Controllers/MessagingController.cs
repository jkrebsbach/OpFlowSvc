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
        public HttpResponseMessage GetCaseMessaging(int? providerId = null, int? locationId = null, int? surgeryId = null, int? caseGroupId = null, int? recipientId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetMessaging(user.UserID, surgeryId, caseGroupId, recipientId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetMessageGroups")]
        [Route("api/message/groups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        public HttpResponseMessage GetCaseMessageGroups(int? providerId = null, int? locationId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetMessageGroups(user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("SendMessage")]
        [Route("api/message")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public HttpResponseMessage Put(int? surgeryId, int? communicationUserId, string message)
        {
            var user = CacheUtil.GetUserSecurity();

            if (surgeryId == null && communicationUserId == null)
                return Request.CreateResponse(HttpStatusCode.Ambiguous);

            DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                surgeryId, communicationUserId, message);

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }
    }
}