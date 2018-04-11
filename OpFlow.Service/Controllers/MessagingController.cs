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
    public class MessagingController : ApiController
    {

        // GET api/values/5
        [SwaggerOperation("GetMessages")]
        [Route("api/message/list")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Messaging>))]
        public HttpResponseMessage GetCaseMessaging(int? userId = null,
            int? surgeryId = null, int? caseGroupId = null, int? recipientId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = DataAccess.SqlHelper.GetMessaging(userId ?? user.UserID, 
                surgeryId, caseGroupId, recipientId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetMessageGroups")]
        [Route("api/message/groups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        public async Task<HttpResponseMessage> GetCaseMessageGroups(int? userId = null, DateTime? surgeryDate = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var groups = DataAccess.SqlHelper.GetMessageGroups(userId ?? user.UserID, surgeryDate, user.ProviderID, user.LocationID);

            foreach (var group in groups.Where(g => g.PatientID.HasValue))
            {
                var patient = await DataAccess.SecureSqlHelper.GetPatient(group.PatientID.Value, user.DatabaseName);

                group.CommunicationTargetName = $"{patient.LastName} {group.CommunicationTargetName}";
            }

            return Request.CreateResponse(HttpStatusCode.OK, groups);
        }

        // GET api/values/5
        [SwaggerOperation("SendMessage")]
        [Route("api/message")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public HttpResponseMessage Put([FromBody]MessagePost messagePost, int? surgeryId = null, int? communicationUserId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            if (surgeryId == null && communicationUserId == null)
                return Request.CreateResponse(HttpStatusCode.Ambiguous);

            DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                surgeryId, communicationUserId, messagePost?.Message);

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }

        // GET api/values/5
        [SwaggerOperation("AcknowledgeMessage")]
        [Route("api/message/acknowledgeMessage")]
        [HttpPut]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public HttpResponseMessage AcknowledgeMessage(int messageId, bool hideMessages = false)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.AcknowledgeMessage(user.UserID, user.ProviderID, user.LocationID, messageId, hideMessages);

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }
    }
}