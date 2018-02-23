using Foundation;
using System;
using UIKit;
using OpFlow.iOS.Delegates;
using System.Threading.Tasks;
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

            await LoadUsers();
            UserSelectTableView.ReloadData();

            if (NavigationDelegate != null)
                NavigationDelegate.DoneEventFired += NavigationDoneFired;
        }

        private async Task SearchUsers()
        {

            var users = await UserUtil.GetUsers(searchUser.Text);

        }

        private void NavigationDoneFired(object sender, EventArgs e)
        {
            // Cancel button - navigate back
        }

        private async Task LoadUsers()
        {
            var users = await SurgeryUtil.GetSurgeryUsers(1);
            var userSelectTableViewSource = new UserSelectionTVS(users);

            UserSelectTableView.Source = userSelectTableViewSource;
        }
    }
}