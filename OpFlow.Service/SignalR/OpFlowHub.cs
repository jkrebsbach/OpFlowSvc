using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.SignalR;

namespace OpFlow.Service.SignalR
{
    public abstract class OpFlowHub : Hub
    {
        private readonly static ConnectionMapping<string> _connections =
            new ConnectionMapping<string>();

        protected static ConnectionMapping<string> Connections => _connections;

        public override Task OnConnected()
        {
            var userAuthId = Context.User.Identity.GetUserId();

            _connections.Add(userAuthId, Context.ConnectionId);

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            var userAuthId = Context.User.Identity.GetUserId();

            _connections.Remove(userAuthId, Context.ConnectionId);

            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            var userAuthId = Context.User.Identity.GetUserId();

            if (!_connections.GetConnections(userAuthId).Contains(Context.ConnectionId))
            {
                _connections.Add(userAuthId, Context.ConnectionId);
            }

            return base.OnReconnected();
        }
    }
}