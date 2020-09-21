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
        protected static ConnectionMapping<string> Connections => new ConnectionMapping<string>();

        public override async Task OnConnected()
        {
            var userAuthId = Context.User.Identity.GetUserId();

            Connections.Add(userAuthId, Context.ConnectionId);

            await ConnectUser(userAuthId);
            await base.OnConnected();
        }

        private async Task ConnectUser(string userAuthId)
        {
            try
            {
                var userTask = await CacheUtil.GetUserSecurity(userAuthId);
                await Groups.Add(Context.ConnectionId, userTask.SelectedLocation.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task DisconnectUser(string userAuthId)
        {
            try
            {
                var userTask = await CacheUtil.GetUserSecurity(userAuthId);

                await Groups.Remove(Context.ConnectionId, userTask.SelectedLocation.ToString());

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public override async Task OnDisconnected(bool stopCalled)
        {
            var userAuthId = Context.User.Identity.GetUserId();

            Connections.Remove(userAuthId, Context.ConnectionId);

            await DisconnectUser(userAuthId);
            await base.OnDisconnected(stopCalled);
        }

        public override async Task OnReconnected()
        {
            var userAuthId = Context.User.Identity.GetUserId();

            if (!Connections.GetConnections(userAuthId).Contains(Context.ConnectionId))
            {
                Connections.Add(userAuthId, Context.ConnectionId);
            }

            await ConnectUser(userAuthId);

            await base.OnReconnected();
        }
    }
}