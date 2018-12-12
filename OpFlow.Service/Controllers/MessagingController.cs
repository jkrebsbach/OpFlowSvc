using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.UI;
using Microsoft.Azure.NotificationHubs;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
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
        public async Task<HttpResponseMessage> GetCaseMessaging(int? userId = null,
            int? surgeryId = null, int? caseGroupId = null, int? recipientId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            var result = await SqlHelper.GetMessaging(userId ?? user.UserID, 
                surgeryId, caseGroupId, recipientId, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, result);
        }

        // GET api/values/5
        [SwaggerOperation("GetMessageGroups")]
        [Route("api/message/groups")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        public async Task<HttpResponseMessage> GetCaseMessageGroups(int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var user = CacheUtil.GetUserSecurity();
            var userObject = SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            var groups = await SqlHelper.GetMessageGroups(userId ?? user.UserID, startDate, endDate, user.ProviderID, user.LocationID);

            foreach (var group in groups.Where(g => g.PatientID.HasValue))
            {
                var patient = await DataAccess.SecureSqlHelper.GetPatient(group.PatientID.Value, 
                    user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID,
                    user.DatabaseName);

                group.CommunicationTargetName = $"{patient.LastName} {group.CommunicationTargetName}";
            }

            groups = groups.OrderByDescending(g => g.SurgeryID.HasValue).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, groups);
        }

        // GET api/values/5
        [SwaggerOperation("SendMessage")]
        [Route("api/message")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<MessagingGroup>))]
        [SwaggerResponse(HttpStatusCode.Ambiguous)]
        public async Task<HttpResponseMessage> Put([FromBody]MessagePost messagePost, int? surgeryId = null, int? communicationUserId = null)
        {
            var user = CacheUtil.GetUserSecurity();

            if ((surgeryId == null && communicationUserId == null) || messagePost == null)
                return Request.CreateResponse(HttpStatusCode.Ambiguous);

            await SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                surgeryId, communicationUserId, messagePost.Message);

            NotificationOutcome notificationOutcome = null;

            if (surgeryId != null)
            {
                var recipients = await SqlHelper.GetSurgeryUsers(surgeryId.Value, user.ProviderID, user.LocationID);

                var sender =
                    SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

                foreach (var recipient in recipients)
                {
                    // don't send message to yourself
                    if (recipient.Email == sender.Email)
                        continue;

                    var surgery = await SqlHelper.GetSurgery(surgeryId ?? -1, user.ProviderID, user.LocationID);
                    var userObject = SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);
                    var patient = await SecureSqlHelper.GetPatient(surgery.PatientID, user.UserID, userObject.FirstName,
                        userObject.LastName, (int)userObject.RoleID, user.DatabaseName);

                    // Prepend surgery descriptor to message
                    var surgeryText =
                        $"MRN: {surgery.CaseNumber} Room: {surgery.RoomDescription} Patient: {patient.Initials} Gender: {patient.Gender} Age: {patient.PatientAge} Message: ";

                    messagePost.Message = surgeryText + messagePost.Message;
                    var senderName = $"{sender.LastName}, {sender.FirstName}";
                    notificationOutcome = await PushNotification.PostNotification(sender.Email, senderName, recipient.Email, messagePost.Message);
                }
            }
            else if (communicationUserId != null)
            {
                var recipientUser =
                    SqlHelper.GetUser(user.ProviderID, user.LocationID,  communicationUserId ?? -1);

                var sender =
                    SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

                var senderName = $"{sender.LastName}, {sender.FirstName}";
                notificationOutcome = await PushNotification.PostNotification(sender.Email, senderName, recipientUser.Email, messagePost.Message);
            }

            return Request.CreateResponse(HttpStatusCode.OK, notificationOutcome);
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

        // GET api/values/5
        [SwaggerOperation("DeletePrivateConversation")]
        [Route("api/message/privateConversation")]
        [HttpDelete]
        [SwaggerResponse(HttpStatusCode.OK)]
        public HttpResponseMessage DeletePrivateConversation(int communicationUserId)
        {
            var user = CacheUtil.GetUserSecurity();

            DataAccess.SqlHelper.DeletePrivateConversation(communicationUserId, user.UserID, user.ProviderID, user.LocationID);

            return Request.CreateResponse(HttpStatusCode.OK, 200);
        }
    }
}