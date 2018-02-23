using Foundation;
using System;
using UIKit;
using OpFlow.iOS.Delegates;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.Mobile;
using OpFlow.iOS.ViewSources;

namespace OpFlow.iOS
{
    public partial class NewCommunicationViewController : OpFlowViewController, INavigationTargetDelegate
    {
        public NewCommunicationViewController (IntPtr handle) : base (handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "CHAT";

            searchUser.SearchButtonClicked += async (sender, e) =>
            {
                ShowPleaseWait("Searching...");
                await SearchUsers();
                HidePleaseWait();
            };

            await SearchUsers();

            if (NavigationDelegate != null)
                NavigationDelegate.DoneEventFired += NavigationDoneFired;
        }

        private async Task SearchUsers()
        {

            var users = await UserUtil.GetUsers(searchUser.Text);
            var userSelectTableViewSource = new UserSelectionTVS(users);
            userSelectTableViewSource.UserSelectionEvent += SelectUser;

            UserSelectTableView.Source = userSelectTableViewSource;

            UserSelectTableView.ReloadData();
        }

        private void NavigationDoneFired(object sender, EventArgs e)
        {
            // Cancel button - navigate back
        }
        private void SelectUser(object sender, User user)
        {
            AppSettings.CurrentMessagingGroup = new MessagingGroup()
            {
                CommunicationUserID = user.UserID
            };

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CommunicationDetail);
        }
    }
}