using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AspNet.SignalR.Client;
using UIKit;

namespace OpFlow.iOS
{
    public class SocketClient
    {
        private readonly string _platform;
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public event EventHandler<string> OnMessageReceived;

        public SocketClient(string platform)
        {
            _platform = platform;
            _connection = new HubConnection("https://opflowservice.azurewebsites.net/signalr");
            _proxy = _connection.CreateHubProxy("appHub");
        }

        public async Task Connect()
        {
            await _connection.Start();

            _proxy.On("messageReceived", (string platform, string message) =>
            {
                if (OnMessageReceived != null)
                    OnMessageReceived(this, string.Format("{0}: {1}", platform, message));
            });

            Send("Connected");
        }

        public Task Send(string message)
        {
            return _proxy.Invoke("Send", _platform, message);
        }
    }
}