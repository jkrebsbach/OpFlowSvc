using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : Activity
    {
        private ProgressDialog _progressDialog;
        private TextView _txtError;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Login);

            _progressDialog = new ProgressDialog(this);
            _progressDialog.SetTitle("Login In Progress");
            _progressDialog.SetMessage("Please wait...");

            var txtUsername = FindViewById<EditText>(Resource.Id.txtUserName);
            var txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            _txtError = FindViewById<EditText>(Resource.Id.txtError);

            var btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            btnLogin.Click += async delegate
            {
                try
                {
                    var username = txtUsername.Text;
                    var password = txtPassword.Text;

                    await AuthenticateUser(username, password);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            };
        }

        private async Task AuthenticateUser(string username, string password)
        {
            RunOnUiThread(() => this._progressDialog.Show());
            
           await WebUtility.LoginUser(username, password);

            RunOnUiThread(() => this._progressDialog.Hide());

            if (!WebUtility.UserAuthenticated)
            {
                _txtError.Text = "Unable to authenticate user";
            }
            else
            {
                // Login successful - Navigate back to application
                OnBackPressed();
            }
        }
    }
}