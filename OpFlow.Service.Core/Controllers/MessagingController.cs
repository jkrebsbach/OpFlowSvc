using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessagingController : OpFlowController
    {
        public MessagingController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        // GET api/values/5
        [Route("api/message/list")]
        public async Task<ActionResult> GetCaseMessaging(int? userId = null,
            int? surgeryId = null, int? caseGroupId = null, int? recipientId = null)
        {
            var user = await GetUserSecurity();
            

            var result = await _sqlHelper.GetMessaging(userId ?? user.UserID, 
                surgeryId, caseGroupId, recipientId, user.SelectedLocation);

            return Ok(result);
        }

        //// GET api/values/5
        //[Route("api/message/groups")]
        //public async Task<ActionResult> GetCaseMessageGroups(int? userId = null, DateTime? startDate = null, DateTime? endDate = null)
        //{
        //    var user = await GetUserSecurity();
            
        //    var userObject = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

        //    var groups = await _sqlHelper.GetMessageGroups(userId ?? user.UserID, startDate, endDate, user.SelectedLocation);

        //    foreach (var group in groups.Where(g => g.PatientID.HasValue))
        //    {
        //        var patient = await secureSqlHelper.GetPatient(group.PatientID.Value, 
        //            user.UserID, userObject.FirstName, userObject.LastName, (int)userObject.RoleID);

        //        group.CommunicationTargetName = $"{patient?.LastName} {group.CommunicationTargetName}";
        //    }

        //    groups = groups.OrderByDescending(g => g.SurgeryID.HasValue).ToList();

        //    return Ok(groups);
        //}

        //// GET api/values/5
        //[Route("api/message")]
        //public async Task<ActionResult> Put([FromBody]MessagePost messagePost, int? surgeryId = null, int? communicationUserId = null)
        //{
        //    var user = await GetUserSecurity();
            
        //    if ((surgeryId == null && communicationUserId == null) || messagePost == null)
        //        return Request.CreateResponse(HttpStatusCode.Ambiguous);

        //    await _sqlHelper.SendMessage(user.UserID, user.SelectedLocation,
        //        surgeryId, communicationUserId, messagePost.Message);

        //    NotificationOutcome notificationOutcome = null;

        //    if (surgeryId != null)
        //    {
        //        var recipients = await _sqlHelper.GetSurgeryUsers(surgeryId.Value, user.SelectedLocation);

        //        var sender = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

        //        foreach (var recipient in recipients)
        //        {
        //            // don't send message to yourself
        //            if (recipient.Email == sender.Email)
        //                continue;

        //            var surgery = await _sqlHelper.GetSurgery(surgeryId ?? -1, user.SelectedLocation);
        //            var userObject = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);
        //            var patient = await secureSqlHelper.GetPatient(surgery.PatientID, user.UserID, userObject.FirstName,
        //                userObject.LastName, (int)userObject.RoleID);

        //            // Prepend surgery descriptor to message
        //            var surgeryText =
        //                $"MRN: {surgery.CaseNumber} Room: {surgery.RoomDescription} Patient: {patient.Initials} Gender: {patient.Gender} Age: {patient.PatientAge} Message: ";

        //            messagePost.Message = surgeryText + messagePost.Message;
        //            var senderName = $"{sender.LastName}, {sender.FirstName}";
        //            notificationOutcome = await PushNotification.PostNotification(sender.Email, senderName, recipient.Email, messagePost.Message);
        //        }
        //    }
        //    else if (communicationUserId != null)
        //    {
        //        var recipientUser = await _sqlHelper.GetUser(user.SelectedLocation,  communicationUserId ?? -1);

        //        var sender = await _sqlHelper.GetUser(user.SelectedLocation,  user.UserID);

        //        var senderName = $"{sender.LastName}, {sender.FirstName}";
        //        notificationOutcome = await PushNotification.PostNotification(sender.Email, senderName, recipientUser.Email, messagePost.Message);
        //    }

        //    return Ok(notificationOutcome);
        //}

        // GET api/values/5
        [Route("api/message/acknowledgeMessage")]
        [HttpPut]
        public async Task<ActionResult> AcknowledgeMessage(int messageId, bool hideMessages = false)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.AcknowledgeMessage(user.UserID, user.SelectedLocation, messageId, hideMessages);

            return Ok(200);
        }

        // GET api/values/5
        [Route("api/message/privateConversation")]
        [HttpDelete]
        public async Task<ActionResult> DeletePrivateConversation(int communicationUserId)
        {
            var user = await GetUserSecurity();
            

            await _sqlHelper.DeletePrivateConversation(communicationUserId, user.UserID, user.SelectedLocation);

            return Ok(200);
        }
    }
}