using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using Mindscape.Raygun4Net;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;

namespace OpFlow.Service.SignalR
{
    [Authorize]
    public class AppHub : OpFlowHub
    {
        public async Task SendSurgeryMessage(int surgeryId, string message)
        {
            try
            {
                var userAuthId = Context.User.Identity.GetUserId();
                var user = await CacheUtil.GetUserSecurity(userAuthId);
                var insertTimestamp = DateTime.Now;

                await SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                    surgeryId, null, message);

                var recipients = await SqlHelper.GetSurgeryUsers(surgeryId, user.ProviderID, user.LocationID);
                var surgery = await SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);
                var userObject = await SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);
                var patient = await SecureSqlHelper.GetPatient(surgery.PatientID, user.UserID, userObject.FirstName,
                    userObject.LastName, (int)userObject.RoleID, user.DatabaseName);

                var sender = userObject;

                var groups = new List<string>()
                {
                    user.ProviderID.ToString()
                };
                Clients.Groups(groups).broadcastMessage(message, (int)sender.RoleID, sender.UserID, sender.DeriveInitials(), insertTimestamp, surgeryId, null);
                //Clients.All.broadcastMessage(message, (int)sender.RoleID, sender.UserID, sender.DeriveInitials(), insertTimestamp, surgeryId, null);

                // Prepend surgery descriptor to message
                var surgeryText =
                    $"MRN: {surgery.CaseNumber} Room: {surgery.RoomDescription} Patient: {patient.Initials} Gender: {patient.Gender} Age: {patient.PatientAge} Message: ";

                message = surgeryText + message;

                foreach (var recipient in recipients)
                {
                    await PushNotificationMessage(sender, recipient.Email, message);
                }
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
            
           
        }

        public async Task SendPrivateMessage(int communicationUserId, string message)
        {
            var userAuthId = Context.User.Identity.GetUserId();
            var user = await CacheUtil.GetUserSecurity(userAuthId);
            var insertTimestamp = DateTime.Now;

            await DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                null, communicationUserId, message);

            var recipientUser = 
                await SqlHelper.GetUser(user.ProviderID, user.LocationID,  communicationUserId);

            var sender =
                await SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            var groups = new List<string>()
            {
                user.ProviderID.ToString()
            };
            Clients.Groups(groups).broadcastMessage(message, (int)sender.RoleID, sender.UserID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);
            //Clients.All.broadcastMessage(message, (int)sender.RoleID, sender.UserID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);

            // Send messages to communication target
            await PushNotificationMessage(sender, recipientUser.Email, message);

            // Send messages to communication source
            await PushNotificationMessage(sender, sender.Email, message);
        }

        private async Task SendNotification(UserSecurity user, User sender, int surgeryId, FlowNotification flowNotification)
        {
            if (flowNotification == null)
                return;

            // Send surgery message in addition to notifications
            await SendSurgeryMessage(surgeryId, flowNotification.FlowMessage);

            SmsNotification.NotifyUser(flowNotification.CellPhone, flowNotification.FlowMessage);

            if (flowNotification.MessagingUserID.HasValue)
            {
                await SendNotification(user, sender, flowNotification.MessagingUserID.Value, flowNotification.FlowMessage);
            }

            if (flowNotification.MessagingRoleID.HasValue)
            {
                var surgeryUsers = await SqlHelper.GetSurgeryUsers(surgeryId, user.ProviderID, user.LocationID);

                foreach (var surgeryUser in surgeryUsers)
                {
                    await SendNotification(user, sender, surgeryUser.UserID, flowNotification.FlowMessage);
                }
            }
        }

        private async Task SendNotification(UserSecurity user, User sender, int targetUserId, string message)
        {
            var recipientUser = await SqlHelper.GetUser(user.ProviderID, user.LocationID, targetUserId);

            await PushNotificationMessage(sender, recipientUser.Email, message);

            var groups = new List<string>()
            {
                user.ProviderID.ToString()
            };
            Clients.Groups(groups).broadcastMessage(message, (int)sender.RoleID, sender.UserID, sender.DeriveInitials(), DateTime.Now, null, targetUserId);
            //Clients.All.broadcastMessage(message, (int)sender.RoleID, sender.UserID, sender.DeriveInitials(), DateTime.Now, null, targetUserId);
            await SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID, null, targetUserId, message);

        }

        private async Task PushNotificationMessage(User sender, string recipientEmail, string message)
        {
            //System.Diagnostics.Debug.WriteLine("SENDING MESSAGE");
            var senderEmail = Context.User.Identity.Name;

            var senderName = $"{sender.LastName}, {sender.FirstName}";

            // Don't send push notification to yourself!
            if (recipientEmail != sender.Email)
                await PushNotification.PostNotification(senderEmail, senderName, recipientEmail, message);
        }



        public async Task AdvanceSurgery(int surgeryId, DateTime stepTime, bool startSurgery)
        {
            try
            {
                var userAuthId = Context.User.Identity.GetUserId();
                var user = await CacheUtil.GetUserSecurity(userAuthId);

                if (startSurgery)
                    await SqlHelper.StartSurgery(surgeryId, user.ProviderID, user.LocationID, stepTime);

                var advanceSurgery = true;
                while (advanceSurgery)
                {
                    var flowStep = await SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID, stepTime);
                    var notifications = await SqlHelper.GetFlowNotifications(flowStep.FlowID, null, user.ProviderID, user.LocationID);

                    var nextNotifications = notifications.Where(n => n.StepID == flowStep.StepID && n.NotificationType == 1);
                    var prevNotifications = notifications.Where(n => n.StepID == flowStep.PreviousStepID && n.NotificationType == 2);

                    var sender = await SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

                    foreach (var nextNotification in nextNotifications)
                        await SendNotification(user, sender, surgeryId, nextNotification); // Next step

                    foreach (var prevNotification in prevNotifications)
                        await SendNotification(user, sender, surgeryId, prevNotification); // Previous step

                    // If the next step has no duration, auto-advance
                    advanceSurgery = (flowStep.StepDuration ?? -1) == 0;
                }

                NotifySurgeryChange(user.ProviderID, "FLOW", surgeryId);
            }
            catch (Exception ex)
            {
                Models.LogHelper.LogException(ex);
                throw;
            }
        }

        public async Task ToggleSurgeryDelay(int surgeryId, DateTime? startTime, DateTime? endTime, int? delayReasonId, string customReason)
        {
            if (startTime == null && endTime == null)
                return;

            var userAuthId = Context.User.Identity.GetUserId();
            var user = await CacheUtil.GetUserSecurity(userAuthId);

            if (customReason != null)
            {
                var surgeryDelayReasonId = await SqlHelper.SurgeryToggleDelayCustom(surgeryId, user.ProviderID, user.LocationID,
                    startTime, endTime, customReason);
            }
            else
            {
                var success = await SqlHelper.SurgeryToggleDelay(surgeryId, user.ProviderID, user.LocationID,
                    startTime, endTime, delayReasonId);
            }

            NotifySurgeryChange(user.ProviderID, "FLOW", surgeryId);
        }

        private void NotifySurgeryChange(int providerId, string property, int surgeryId)
        {
            var groups = new List<string>()
            {
                providerId.ToString()
            };
            Clients.Groups(groups).surgeryChange(property, surgeryId);
            //Clients.All.surgeryChange(property, surgeryId);
        }
    }
}