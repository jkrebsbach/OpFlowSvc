using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;
using OpFlow.Service.Models;

namespace OpFlow.Service.SignalR
{
    [Authorize]
    public class AppHub : OpFlowHub
    {
        public async Task SendSurgeryMessage(int surgeryId, string message)
        {
            var user = CacheUtil.GetUserByEmail();
            var insertTimestamp = DateTime.Now;
            
            await DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                surgeryId, null, message);

            var recipients = await DataAccess.SqlHelper.GetSurgeryUsers(surgeryId, user.ProviderID, user.LocationID);
            var surgery = DataAccess.SqlHelper.GetSurgery(surgeryId, user.ProviderID, user.LocationID);
            var userObject = DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);
            var patient = await DataAccess.SecureSqlHelper.GetPatient(surgery.PatientID, user.UserID, userObject.FirstName,
                userObject.LastName, (int)userObject.RoleID, user.DatabaseName);

            var sender =
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);

            
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

        public async Task SendPrivateMessage(int communicationUserId, string message)
        {
            var user = CacheUtil.GetUserByEmail();
            var insertTimestamp = DateTime.Now;

            await DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                null, communicationUserId, message);

            var recipientUser = 
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, communicationUserId);

            var sender =
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);

            Clients.All.broadcastMessage(message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);

            // Send messages to communication target
            await SendMessage(recipientUser.Email, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, sender.UserID);

            // Send messages to communication source
            await SendMessage(HttpContext.Current.User.Identity.GetUserName(), message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);
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