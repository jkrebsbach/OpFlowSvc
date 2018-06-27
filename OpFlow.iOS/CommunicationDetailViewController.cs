using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;
using System.Collections.Generic;

namespace OpFlow.iOS
{
    public partial class CommunicationDetailViewController : OpFlowViewController
    {
        private readonly SocketClient _client;
        private List<Messaging> _messages;

        public CommunicationDetailViewController (IntPtr handle) : base (handle)
        {
            _client = new SocketClient("iOS");
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            _client.Disconnect();
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);


            await _client.Connect();
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            _client.OnMessageReceived += (sender, message) => InvokeOnMainThread(
                () => {
                var currentMessageGroup = AppSettings.CurrentMessagingGroup;

                var newMessage = new Messaging()
                {
                    SenderRoleID = (RoleEnum)message.SenderRoleID,
                    UserName = message.SenderUserName,
                    Message = message.Message,
                    InsertTimestamp = message.InsertTimestamp
                };

                // Is this message part of the current conversation?
                if (message.SurgeryID == currentMessageGroup.SurgeryID &&
                    message.CommunicationUserID == currentMessageGroup.CommunicationUserID)
                {
                    _messages.Add(newMessage);
                    CommunicatorTableView.ReloadData();

                    var detailIndexPath = NSIndexPath.FromRowSection(_messages.Count - 1, 0);
                    CommunicatorTableView.ScrollToRow(detailIndexPath, UITableViewScrollPosition.None, true);
                }
            });

            SetupDoneStyleTextField(txtMessage);

            //Title = "SCHEDULEVIEW";
            await ExecuteAsyncWebRequest(LoadMessages);
        }

        private async Task LoadMessages()
        {
            var currentMessageGroup = AppSettings.CurrentMessagingGroup;

            _messages = await MessagingUtil.GetMessages(currentMessageGroup.SurgeryID, currentMessageGroup.CaseGroupID, currentMessageGroup.CommunicationUserID);

            var messageTableViewSource = new CommunicatorTVS(_messages);

            CommunicatorTableView.Source = messageTableViewSource;
            CommunicatorTableView.ReloadData();

            // If we have any messages, scroll to bottom of message stack
            if (_messages.Count > 0)
            {
                var detailIndexPath = NSIndexPath.FromRowSection(_messages.Count - 1, 0);
                CommunicatorTableView.ScrollToRow(detailIndexPath, UITableViewScrollPosition.None, true);
            }
        }

        async partial void btnRefresh_Click(UIButton sender)
        {
            await ExecuteAsyncWebRequest(LoadMessages);
        }

        async partial void btnSendMessage_Click(UIKit.UIButton sender)
        {
            await ExecuteAsyncWebRequest(SendMessage);
        }

        private async Task SendMessage()
        {
            try
            {
                if (txtMessage.Text == "")
                    return;

                //await MessagingUtil.SendMessage(AppSettings.CurrentMessagingGroup, txtMessage.Text);

                if (AppSettings.CurrentMessagingGroup.SurgeryID.HasValue)
                    await _client.SendSurgeryMessage(AppSettings.CurrentMessagingGroup.SurgeryID.Value, txtMessage.Text);
                else
                    await _client.SendPrivateMessage(AppSettings.CurrentMessagingGroup.CommunicationUserID.Value, txtMessage.Text);

                txtMessage.ResignFirstResponder();

                txtMessage.Text = string.Empty;

                //await LoadMessages();
                
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}