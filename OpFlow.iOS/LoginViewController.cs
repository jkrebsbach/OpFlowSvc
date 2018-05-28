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

namespace OpFlow.iOS
{
    public partial class LoginViewController : OpFlowViewController
    {
        public LoginViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

			var account = await GetCurrentCredential();
			if (account != null){
				txtUsername.Text = account.Username;
			}

            //  sizing issues moving windows up and down - just keep it all down
            //NavigationItem.SetHidesBackButton(true, false);
            //NavigationController.NavigationBar.Hidden = true;
        }

		UIAlertController _loginError = null;

        async partial void SignOnClick(UIButton sender)
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
            await AppSettings.AuthenticateUser(txtUsername.Text, txtPassword.Text);

            if (AppSettings.UserAuthenticated)
            {
				await SetCredential(txtUsername.Text, string.Empty);

                await RegisterDevice(txtUsername.Text);

                // Navigate to schedule page after signon
                AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;

                var controller = Storyboard.InstantiateViewController("MainViewController");
                NavigationController.PushViewController(controller, true);
            }
        }

		private const string _appId = "OpFlow";

        private static async Task SetCredential(string userName, string password)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var store = AccountStore.Create();
                var account = (await store.FindAccountsForServiceAsync(_appId)).FirstOrDefault() ?? new Account();

                account.Username = userName;
                account.Properties["Password"] = password;
                await store.SaveAsync(account, _appId);
            }
        }

        private static async Task RegisterDevice(string userName)
        {
            var deviceToken = NSUserDefaults.StandardUserDefaults["PushDeviceToken"];

            if (deviceToken != null && deviceToken.ToString() != null){
                var token = deviceToken.ToString();

                var registration = await UserUtil.RegisterDevice(token);

                var deviceUpdate = new NotificationDeviceRegistration()
                {
                    Platform = "apns",
                    Handle = token
                };
                var listener = await UserUtil.RegisterUser(registration, deviceUpdate);
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