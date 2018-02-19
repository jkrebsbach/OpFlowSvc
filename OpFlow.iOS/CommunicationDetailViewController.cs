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
    public partial class CommunicationDetailViewController : OpFlowViewController, INavigationTargetDelegate
    {
        public CommunicationDetailViewController (IntPtr handle) : base (handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            //Title = "SCHEDULEVIEW";
            await LoadMessages();
        }

        private async Task LoadMessages()
        {
            var currentMessageGroup = AppSettings.CurrentMessagingGroup;

            var messages = await MessagingUtil.GetMessages(currentMessageGroup.CaseID, currentMessageGroup.CaseGroupID, currentMessageGroup.CommunicationUserID);

            var messageTableViewSource = new CommunicatorTVS(messages);
            
            CommunicatorTableView.Source = messageTableViewSource;

            CommunicatorTableView.ReloadData();
        }
    }
}