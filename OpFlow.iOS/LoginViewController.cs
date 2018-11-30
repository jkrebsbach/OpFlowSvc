using Foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;
using Xamarin.Auth;
using OpFlow.Data;
using OpFlow.iOS.Helpers;

namespace OpFlow.iOS
{
    public partial class LoginViewController : OpFlowViewController
    {
        String ServiceID = "OPFLOW_MOBILE";
        String password = null;

        public LoginViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.NavigationBar.Hidden = true;

            base.ViewWillAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.NavigationBar.Hidden = false;

            base.ViewWillDisappear(animated);
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupDoneStyleTextField(txtUsername, true);
            SetupDoneStyleTextField(txtPassword, false);


            var account = await GetCurrentCredential();
            if (account != null)
            {
                txtUsername.Text = account.Username;

                password = KeychainHelper.GetPasswordForUsername(txtUsername.Text,
                                                                    ServiceID,
                                                                     true);

                if (!string.IsNullOrEmpty(password))
                {
                    LocalAuthHelper.Authenticate(AuthSuccess, AuthFailure);
                }
            }
        }

        private void AuthSuccess()
        {
            InvokeOnMainThread(async () =>
            {
                await ExecuteSignOn();
            });
        }

        private void AuthFailure()
        {
            InvokeOnMainThread(() =>
            {
                password = string.Empty;
                _loginError = ShowDialog("Error", "Issues retrieving biometric authentication");
            });
        }

        UIAlertController _loginError = null;

        async partial void SignOnClick(UIButton sender)
        {
            password = txtPassword.Text;
            await ExecuteSignOn();
        }

        private async Task ExecuteSignOn()
        {
            await ExecuteAsyncWebRequest(SignOn, "Authenticating...");

            if (!AppSettings.UserAuthenticated)
            {
                await Task.Delay(500); // something goofy about modal dialogs?...

                if (_loginError != null)
                {
                    _loginError.DismissViewController(false, null);
                }
                _loginError = ShowDialog("Error", "Please enter a valid username and password");
                txtPassword.Text = "";
            }
        }

        private async Task SignOn()
        {
            AppDelegate.DisposeSocketClient();
            await AppSettings.AuthenticateUser(txtUsername.Text, password);

            if (AppSettings.UserAuthenticated)
            {
                await SetCredential(txtUsername.Text);

                KeychainHelper.SetPasswordForUsername(
                    txtUsername.Text, txtPassword.Text, ServiceID, Security.SecAccessible.WhenPasscodeSetThisDeviceOnly, true);

                await RegisterDevice();

                // Navigate to schedule page after signon
                AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;

                var controller = Storyboard.InstantiateViewController("MainViewController");
                NavigationController.PushViewController(controller, true);
            }
        }

        private const string _appId = "OpFlow";

        private static async Task SetCredential(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var store = AccountStore.Create();
                var account = (await store.FindAccountsForServiceAsync(_appId)).FirstOrDefault() ?? new Account();

                account.Username = userName;
                await store.SaveAsync(account, _appId);
            }
        }

        private static async Task RegisterDevice()
        {
            var deviceToken = NSUserDefaults.StandardUserDefaults["PushDeviceToken"];

            if (deviceToken != null && deviceToken.ToString() != ""){
                var token = deviceToken.ToString();

                await UserUtil.RegisterDevice(token, "apns");
            }
        }

        private static async Task<Account> GetCurrentCredential()
        {
            var store = AccountStore.Create();
            var length = store.FindAccountsForService(_appId).Count();
            var account = store.FindAccountsForService(_appId).FirstOrDefault();

            while (length > 1)
            {
                await store.DeleteAsync(account, _appId);

                length = store.FindAccountsForService(_appId).Count();
                account = store.FindAccountsForService(_appId).FirstOrDefault();
            }

            return account;
        }
    }
}