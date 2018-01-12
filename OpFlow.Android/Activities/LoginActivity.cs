using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Mobile;

namespace OpFlow.Android.Activities
{
    [Activity(Label = "Sign On")]
    public class LoginActivity : Activity
    {
        private ProgressDialog _progressDialog;
        private EditText _txtUsername;
        private EditText _txtPassword;
        private TextView _txtError;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);

            _progressDialog = new ProgressDialog(this);
            _progressDialog.SetTitle("Login In Progress");
            _progressDialog.SetMessage("Please wait...");

            _txtUsername = FindViewById<EditText>(Resource.Id.txtUserName);
            _txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            _txtError = FindViewById<TextView>(Resource.Id.txtError);
            var btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            btnLogin.Click += async delegate
            {
                try
                {
                    var username = _txtUsername.Text;
                    var password = _txtPassword.Text;

                    await AuthenticateUser(username, password);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };

            var task = LoadCredentials();
            task.Wait();
        }

        private async Task LoadCredentials()
        {
            var credentials = await AndroidApp.GetCurrentCredential();
            if (credentials != null)
            {
                _txtUsername.Text = credentials.Username;
                _txtPassword.Text = credentials.Properties.ContainsKey("Password") ?
                    credentials.Properties["Password"] : "";
            }
        }

        private async Task AuthenticateUser(string username, string password)
        {
            _progressDialog.Show();

            await AppSettings.AuthenticateUser(username, password);

            _progressDialog.Hide();

            if (!AppSettings.UserAuthenticated)
            {
                _txtError.Text = "Unable to authenticate user";
            }
            else
            {
                await AndroidApp.SetCredential(username, password);

                // Login successful - Navigate back to application
                OnBackPressed();
            }
        }
    }
}