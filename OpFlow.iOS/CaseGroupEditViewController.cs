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
    public partial class CaseGroupEditViewController : OpFlowViewController, INavigationTargetDelegate
    {
        public CaseGroupEditViewController (IntPtr handle) : base (handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "SCHEDULEVIEW";

            await LoadUsers();
            UserSelectTableView.ReloadData();

            if (NavigationDelegate != null)
                NavigationDelegate.DoneEventFired += NavigationDoneFired;
        }

        private async void NavigationDoneFired(object sender, EventArgs e)
        {
            var userData = UserSelectTableView.Source as UserSelectionTVS;

            var userSelections = userData?.SelectedUsers();

            if ((userSelections?.Count ?? 0) == 0)
            {
                ShowDialog("Error", "No users selected");
                return;
            }

            var messagingGroup = await MessagingUtil.CreateGroup(1, userSelections);

            AppSettings.CurrentMessagingGroup = messagingGroup;

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CommunicationDetail);
        }

        private async Task LoadUsers()
        {
            var users = await SurgeryUtil.GetSurgeryUsers(1);
            var userSelectTableViewSource = new UserSelectionTVS(users);

            UserSelectTableView.Source = userSelectTableViewSource;
        }
    }
}