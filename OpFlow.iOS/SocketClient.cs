using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using Microsoft.AspNet.SignalR.Client;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public class SocketClient
    {
        private readonly string _platform;
        private readonly HubConnection _connection;
        private readonly IHubProxy _proxy;

        public class MessageReceiveEvent
        {
            public string Message { get; set; }
            public int SenderRoleID { get; set; }
            public string SenderUserName { get; set; }
            public DateTime InsertTimestamp { get; set; }
            public int? SurgeryID { get; set; }
            public int? CommunicationUserID { get; set; }
        }

        public event EventHandler<MessageReceiveEvent> OnMessageReceived;

        public SocketClient(string platform)
        {
            _platform = platform;
            _connection = new HubConnection("https://opflowservice.azurewebsites.net/signalr");
            _connection.Headers.Add("Authorization", "Bearer " + AppSettings.AuthenticationToken);
            _proxy = _connection.CreateHubProxy("appHub");
        }

        public async Task Connect()
        {
            await _connection.Start();

            _proxy.On("broadcastMessage", (string message, int senderRoleId, string senderUserName, DateTime insertTimestamp,
                                          int? surgeryId, int? communicationUserId) =>
            {
                OnMessageReceived?.Invoke(this, new MessageReceiveEvent()
                {
                    Message = message,
                    SenderRoleID = senderRoleId,
                    SenderUserName = senderUserName,
                    InsertTimestamp = insertTimestamp,
                    SurgeryID = surgeryId,
                    CommunicationUserID = communicationUserId
                });
            });

            //Send("Connected");
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        public Task SendSurgeryMessage(int surgeryId, string message)
        {
            return _proxy.Invoke("sendSurgeryMessage", surgeryId, message);
        }

        public Task SendPrivateMessage(int communicationUserId, string message)
        {
            return _proxy.Invoke("sendPrivateMessage", communicationUserId, message);
        }
    }
}