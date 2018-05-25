using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Cors;
using Microsoft.AspNet.SignalR;
using OpFlow.Service.SignalR;

namespace OpFlow.Service
{
    [Authorize]
    public class AppHub : OpFlowHub
    {
        public void Send(string name, string message)
        {
            //Clients.All.broadcastMessage(name, message);

            var recipients = new [] {"ben@opflowtech.com"};
            foreach (var recipient in recipients)
            {
                SendMessage(recipient, message);
            }
        }

        private void SendMessage(string who, string message)
        {
            var name = Context.User.Identity.Name;

            foreach (var connectionId in Connections.GetConnections(who))
            {
                Clients.Client(connectionId).broadcastMessage(name, message);
            }
        }
    }
}