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
    public partial class NewCommunicationViewController : OpFlowViewController
    {
        public NewCommunicationViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            Title = "CHAT";

            searchUser.SearchButtonClicked += async (sender, e) =>
            {
                await SearchUsers();
            };

            await SearchUsers();
        }

        private async Task SearchUsers()
        {
            await ExecuteAsyncWebRequest(SearchUsersWrapper());
        }

        private async Task SearchUsersWrapper()
        {
            var users = await UserUtil.GetUsers(searchUser.Text);
            var userSelectTableViewSource = new UserSelectionTVS(users);
            userSelectTableViewSource.UserSelectionEvent += SelectUser;

            UserSelectTableView.Source = userSelectTableViewSource;

            UserSelectTableView.ReloadData();
        }
        
        private void SelectUser(object sender, User user)
        {
            AppSettings.CurrentMessagingGroup = new MessagingGroup()
            {
                CommunicationUserID = user.UserID,
                CommunicationTargetName = string.Format("{0}: {1}, {2}", user.RoleID, user.LastName, user.FirstName)
            };

            // back button should go to communicator home, not this screen
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Communicator;

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CommunicationDetail);
        }
    }
}