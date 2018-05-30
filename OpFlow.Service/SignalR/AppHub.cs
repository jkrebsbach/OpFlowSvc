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
            
            DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                surgeryId, null, message);

            var recipients = DataAccess.SqlHelper.GetSurgeryUsers(surgeryId, user.ProviderID, user.LocationID);

            var sender =
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);

            foreach (var recipient in recipients)
            {
                await SendMessage(recipient.Email, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, surgeryId, null);
            }
        }

        public async Task SendPrivateMessage(int communicationUserId, string message)
        {
            var user = CacheUtil.GetUserByEmail();
            var insertTimestamp = DateTime.Now;

            DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                null, communicationUserId, message);

            var recipientUser = 
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, communicationUserId);

            var sender =
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, user.UserID);

            var recipients = new[] { recipientUser.Email, HttpContext.Current.User.Identity.GetUserName() };
            foreach (var recipient in recipients)
            {
                await SendMessage(recipient, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);
            }
        }

        private async Task SendMessage(string who, string message, int senderRoleId, string senderUserName, DateTime insertTimestamp, int? surgeryId, int? communicationUserId)
        {
            var name = Context.User.Identity.Name;

            await PushNotification.PostNotification(name, who, message);

            foreach (var connectionId in Connections.GetConnections(who))
            {
                Clients.Client(connectionId).broadcastMessage(message, senderRoleId, senderUserName, insertTimestamp, surgeryId, communicationUserId);
            }
        }
    }
}