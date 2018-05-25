using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Cors;
using Microsoft.AspNet.SignalR;

namespace OpFlow.Service
{
    [Authorize]
    public class AppHub : Hub
    {
        public void Send(string name, string message)
        {
            Clients.All.broadcastMessage(name, message);
        }
    }
}