using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;
using System.Collections.Generic;

namespace OpFlow.iOS
{
    public partial class CommunicatorViewController : OpFlowViewController
    {
        private readonly SocketClient _client;
        private List<MessagingGroup> _messageGroups;

        public CommunicatorViewController (IntPtr handle) : base (handle)
        {
            _client = new SocketClient("iOS");
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            await _client.Connect();

            _client.OnMessageReceived += (sender, message) => InvokeOnMainThread(
                () =>
                {
                    var messageGroup = _messageGroups.FirstOrDefault(m =>
                                                                    m.SurgeryID == message.SurgeryID &&
                                                                     m.CommunicationUserID == message.CommunicationUserID);

                    if (messageGroup != null)
                    {
                        messageGroup.LatestMessage = message.Message;
                        messageGroup.LatestInsertTimestamp = message.InsertTimestamp;
                    }
                    else
                    {
                        messageGroup = new MessagingGroup()
                        {
                            SurgeryID = message.SurgeryID,
                            CommunicationUserID = message.CommunicationUserID,
                            CommunicationTargetName = message.SenderUserName,
                            LatestMessage = message.Message,
                            LatestInsertTimestamp = message.InsertTimestamp
                        };    
                    }

                    CommunicatorTableView.ReloadData();
                });
            

            //Title = "SCHEDULEVIEW";
            await ExecuteAsyncWebRequest(LoadMessageGroups);
        }

        private async Task LoadMessageGroups()
        {
            _messageGroups = await MessagingUtil.GetMessageGroups();

            var messageGroupTableViewSource = new CommunicationGroupTVS(_messageGroups);
            messageGroupTableViewSource.MessageGroupSelectionEvent += SelectMessageGroup;

            CommunicatorTableView.Source = messageGroupTableViewSource;

            CommunicatorTableView.ReloadData();
        }

        private void SelectMessageGroup(object sender, MessagingGroup messageGroup)
        {
            AppSettings.CurrentMessagingGroup = messageGroup;

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CommunicationDetail);
        }
    }
}