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
    public partial class CommunicationDetailViewController : OpFlowViewController
    {
        public CommunicationDetailViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupDoneStyleTextField(txtMessage);

            //Title = "SCHEDULEVIEW";
            await ExecuteAsyncWebRequest(LoadMessages);
        }

        private async Task LoadMessages()
        {
            var currentMessageGroup = AppSettings.CurrentMessagingGroup;

            var messages = await MessagingUtil.GetMessages(currentMessageGroup.SurgeryID, currentMessageGroup.CaseGroupID, currentMessageGroup.CommunicationUserID);

            var messageTableViewSource = new CommunicatorTVS(messages);

            CommunicatorTableView.Source = messageTableViewSource;
            CommunicatorTableView.ReloadData();

            // If we have any messages, scroll to bottom of message stack
            if (messages.Count > 0)
            {
                var detailIndexPath = NSIndexPath.FromRowSection(messages.Count - 1, 0);
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
            if (txtMessage.Text == "")
                return;

            await MessagingUtil.SendMessage(AppSettings.CurrentMessagingGroup, txtMessage.Text);
            txtMessage.ResignFirstResponder();

            txtMessage.Text = string.Empty;

            await LoadMessages();
        }
    }
}