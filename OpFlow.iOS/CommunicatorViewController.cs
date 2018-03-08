using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CommunicatorViewController : OpFlowViewController
    {
        public CommunicatorViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Title = "SCHEDULEVIEW";
            await ExecuteAsyncWebRequest(LoadMessageGroups());
        }

        private async Task LoadMessageGroups()
        {
            var messageGroups = await MessagingUtil.GetMessageGroups();

            var messageGroupTableViewSource = new CommunicationGroupTVS(messageGroups);
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