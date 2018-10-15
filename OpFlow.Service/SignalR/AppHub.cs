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
                var user = CacheUtil.GetUserByEmail(Context.User.Identity.Name);
                var insertTimestamp = DateTime.Now;

                await DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                    surgeryId, null, message);

                var recipients = await DataAccess.SqlHelper.GetSurgeryUsers(surgeryId, user.ProviderID, user.LocationID);
                var surgery = DataAccess.SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);
                var userObject = DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);
                var patient = await DataAccess.SecureSqlHelper.GetPatient(surgery.PatientID, user.UserID, userObject.FirstName,
                    userObject.LastName, (int)userObject.RoleID, user.DatabaseName);

                var sender = userObject;


                Clients.All.broadcastMessage(message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, surgeryId, null);

                // Prepend surgery descriptor to message
                var surgeryText =
                    $"MRN: {surgery.CaseNumber} Room: {surgery.RoomDescription} Patient: {patient.Initials} Gender: {patient.Gender} Age: {patient.PatientAge} Message: ";

                message = surgeryText + message;

                foreach (var recipient in recipients)
                {
                    await SendMessage(recipient.Email, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, surgeryId, null);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                RaygunClient client = new RaygunClient("f12C1dpwvycqBLOm2YT5rw==");
                client.Send(e);
                throw;
            }
            
           
        }

        public async Task SendPrivateMessage(int communicationUserId, string message)
        {
            var user = CacheUtil.GetUserByEmail(Context.User.Identity.Name);
            var insertTimestamp = DateTime.Now;

            await DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                null, communicationUserId, message);

            var recipientUser = 
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID,  communicationUserId);

            var sender =
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID,  user.UserID);

            Clients.All.broadcastMessage(message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);

            // Send messages to communication target
            await SendMessage(recipientUser.Email, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, sender.UserID);

            // Send messages to communication source
            await SendMessage(HttpContext.Current.User.Identity.GetUserName(), message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);
        }

        public async Task AdvanceSurgery(int surgeryId, DateTime stepTime, bool startSurgery)
        {
            var user = CacheUtil.GetUserByEmail(Context.User.Identity.Name);

            if (startSurgery)
                SqlHelper.StartSurgery(surgeryId, user.ProviderID, user.LocationID, stepTime);
            
            var flowStep = SqlHelper.SurgeryMoveNextStep(surgeryId, user.ProviderID, user.LocationID, stepTime);
            var notifications = SqlHelper.GetFlowNotifications(flowStep.FlowID, null, user.ProviderID, user.LocationID);

            var nextNotification = notifications.FirstOrDefault(n => n.StepID == flowStep.StepID && n.NotificationType == 1);
            var prevNotification = notifications.FirstOrDefault(n => n.StepID == flowStep.PreviousStepID && n.NotificationType == 2);

            var sender = SqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

            await SendNotification(user, sender, nextNotification); // Next step
            await SendNotification(user, sender, prevNotification); // Previous step
        }

        private async Task SendNotification(UserSecurity user, User sender, FlowNotification flowNotification)
        {
            if (flowNotification == null)
                return;

            SmsNotification.NotifyUser(flowNotification.CellPhone, flowNotification.FlowMessage);

            if (flowNotification.MessagingUserID != null)
            {
                var recipientUser =
                    DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, flowNotification.MessagingUserID.Value);

                await SendMessage(recipientUser.Email, flowNotification.FlowMessage, (int)sender.RoleID, sender.DeriveInitials(), DateTime.Now, null, sender.UserID);

                Clients.All.broadcastMessage(flowNotification.FlowMessage, 
                    (int)sender.RoleID, sender.DeriveInitials(), DateTime.Now, null, flowNotification.MessagingUserID);
                await SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID, null, flowNotification.MessagingUserID, flowNotification.FlowMessage);

            }
        }

        private async Task SendMessage(string who, string message, int senderRoleId, string senderUserName, DateTime insertTimestamp, int? surgeryId, int? communicationUserId)
        {
            //System.Diagnostics.Debug.WriteLine("SENDING MESSAGE");
            var name = Context.User.Identity.Name;

            // Don't send push notification to yourself!
            if (name != who)
                await PushNotification.PostNotification(name, who, message);

            //Clients.All.broadcastMessage(message, senderRoleId, senderUserName, insertTimestamp, surgeryId, communicationUserId);

            //foreach (var connectionId in Connections.GetConnections(who))
            //{
            //    Clients.Client(connectionId).broadcastMessage(message, senderRoleId, senderUserName, insertTimestamp, surgeryId, communicationUserId);
            //}
        }
    }
}