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
            public int SenderUserID { get; set; }
            public string SenderUserName { get; set; }
            public DateTime InsertTimestamp { get; set; }
            public int? SurgeryID { get; set; }
            public int? CommunicationUserID { get; set; }
            public int? TrayProposalID { get; set; }
        }

        public event EventHandler<MessageReceiveEvent> OnMessageReceived;

        private static string _devUrl = "https://opflowservice.azurewebsites.net/signalr";
        private static string _prodUrl = "https://opflowsvc.azurewebsites.net/signalr";
        private static bool _productionEnvironment;

        public SocketClient(string platform)
        {
            _platform = platform;
            _connection = new HubConnection(_productionEnvironment ? _prodUrl : _devUrl);
            _connection.Headers.Add("Authorization", "Bearer " + AppSettings.AuthenticationToken);
            _proxy = _connection.CreateHubProxy("appHub");
        }

        public static void SetEnvironment(bool isProduction)
        {
            _productionEnvironment = isProduction;
        }

        public async Task Connect()
        {
            if (_connection.State == ConnectionState.Connected)
                return;

            await _connection.Start();

            _proxy.On("broadcastMessage", (string message, int senderRoleId, int senderUserId,
                string senderUserName, DateTime insertTimestamp,
                int? surgeryId, int? communicationUserId, int? trayProposalId) =>
            {
                OnMessageReceived?.Invoke(this, new MessageReceiveEvent()
                {
                    Message = message,
                    SenderRoleID = senderRoleId,
                    SenderUserID = senderUserId,
                    SenderUserName = senderUserName,
                    InsertTimestamp = insertTimestamp,
                    SurgeryID = surgeryId,
                    CommunicationUserID = communicationUserId,
                    TrayProposalID = trayProposalId
                });
            });

            //Send("Connected");
        }

        public void Disconnect()
        {
            if (_connection != null)
                _connection.Dispose();
        }

        public async Task SendSurgeryMessage(int surgeryId, string message)
        {
            await _proxy.Invoke("sendSurgeryMessage", surgeryId, message);
        }

        public async Task SendPrivateMessage(int communicationUserId, string message)
        {
            try
            {
                await _proxy.Invoke("sendPrivateMessage", communicationUserId, message);
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}