using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace OpFlow.Service.SignalR
{
    [Authorize]
    public class AppHub : OpFlowHub
    {
        public void SendSurgeryMessage(int surgeryId, string message)
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
                SendMessage(recipient.Email, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, surgeryId, null);
            }
        }

        public void SendPrivateMessage(int communicationUserId, string message)
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
                SendMessage(recipient, message, (int)sender.RoleID, sender.DeriveInitials(), insertTimestamp, null, communicationUserId);
            }
        }

        private void SendMessage(string who, string message, int senderRoleId, string senderUserName, DateTime insertTimestamp, int? surgeryId, int? communicationUserId)
        {
            var name = Context.User.Identity.Name;

            foreach (var connectionId in Connections.GetConnections(who))
            {
                Clients.Client(connectionId).broadcastMessage(message, senderRoleId, senderUserName, insertTimestamp, surgeryId, communicationUserId);
            }
        }
    }
}