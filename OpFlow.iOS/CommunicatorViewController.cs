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
using static OpFlow.iOS.SocketClient;

namespace OpFlow.iOS
{
    public partial class CommunicatorViewController : OpFlowViewController
    {
        private List<MessagingGroup> _messageGroups;

        NSObject _notificationObserver;

        public CommunicatorViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            AppDelegate.AppSocketClient.OnMessageReceived -= SocketMessageReceived;
            NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationObserver);
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            _notificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, RefreshSocket);
            await AppDelegate.SetupSocketClient();

            AppDelegate.AppSocketClient.OnMessageReceived += SocketMessageReceived;
        }


        async void RefreshSocket(NSNotification notification)
        {
            await AppDelegate.SetupSocketClient();
            await ExecuteAsyncWebRequest(LoadMessageGroups);
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();


            //Title = "SCHEDULEVIEW";
            await ExecuteAsyncWebRequest(LoadMessageGroups);
        }

        private void SocketMessageReceived(Object sender, MessageReceiveEvent message) {
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

                _messageGroups.Add(messageGroup);
            }

            InvokeOnMainThread(() => {
                RefreshTables();
            });
        }

        private async Task LoadMessageGroups()
        {
            var onlyToday = swtCaseFilter.On;

            _messageGroups = await MessagingUtil.GetMessageGroups(onlyToday);

            RefreshTables();
        }

        private void RefreshTables()
        {

            var surgeryMessageGroupTableViewSource =
                new CommunicationGroupTVS(_messageGroups.Where(m => m.SurgeryID.HasValue).ToList());
            surgeryMessageGroupTableViewSource.MessageGroupSelectionEvent += SelectMessageGroup;

            SurgeryCommunicatorTableView.Source = surgeryMessageGroupTableViewSource;
            SurgeryCommunicatorTableView.ReloadData();

            var privateMessageGroupTableViewSource =
                new CommunicationGroupTVS(_messageGroups.Where(m => !m.SurgeryID.HasValue).ToList());
            privateMessageGroupTableViewSource.MessageGroupSelectionEvent += SelectMessageGroup;

            PrivateCommunicatorTableView.Source = privateMessageGroupTableViewSource;
            PrivateCommunicatorTableView.ReloadData();
        }

        async partial void swtCaseFilter_Changed(UISwitch sender)
        {
            await ExecuteAsyncWebRequest(LoadMessageGroups);
        }

        private void SelectMessageGroup(object sender, MessagingGroup messageGroup)
        {
            AppSettings.CurrentMessagingGroup = messageGroup;

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CommunicationDetail);
        }
    }
}