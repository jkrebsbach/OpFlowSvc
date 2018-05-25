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

            DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                surgeryId, null, message);

            var recipients = DataAccess.SqlHelper.GetSurgeryUsers(surgeryId,
                user.ProviderID, user.LocationID);

            foreach (var recipient in recipients)
            {
                SendMessage(recipient.Email, message, surgeryId, null);
            }
        }

        public void SendPrivateMessage(int communicationUserId, string message)
        {
            var user = CacheUtil.GetUserByEmail();

            DataAccess.SqlHelper.SendMessage(user.UserID, user.ProviderID, user.LocationID,
                null, communicationUserId, message);

            var recipientUser = 
                DataAccess.SqlHelper.GetUser(user.ProviderID, user.LocationID, null, communicationUserId);

            var recipients = new[] { recipientUser.Email, HttpContext.Current.User.Identity.GetUserName() };
            foreach (var recipient in recipients)
            {
                SendMessage(recipient, message, null, communicationUserId);
            }
        }

        private void SendMessage(string who, string message, int? surgeryId, int? communicationUserId)
        {
            var name = Context.User.Identity.Name;

            foreach (var connectionId in Connections.GetConnections(who))
            {
                Clients.Client(connectionId).broadcastMessage(message, surgeryId, communicationUserId);
            }
        }
    }
}