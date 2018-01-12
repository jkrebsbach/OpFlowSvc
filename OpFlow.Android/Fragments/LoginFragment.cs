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
using OpFlow.Android.Activities;
using OpFlow.Mobile;

namespace OpFlow.Android.Fragments
{
    public class LoginFragment : OpFlowFragmentBase
    {
        private ProgressDialog _progressDialog;
        private TextView _txtError;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.Login, container, false);

            _progressDialog = new ProgressDialog(Context);
            _progressDialog.SetTitle("Login In Progress");
            _progressDialog.SetMessage("Please wait...");

            var txtUsername = rootView.FindViewById<EditText>(Resource.Id.txtUserName);
            var txtPassword = rootView.FindViewById<EditText>(Resource.Id.txtPassword);
            _txtError = rootView.FindViewById<EditText>(Resource.Id.txtError);

            var btnLogin = rootView.FindViewById<Button>(Resource.Id.btnLogin);
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

            return rootView;
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
                // Login successful - Navigate back to application
                Listener.SendMessage(AppSettings.FragmentEnum.Login, true);
            }
        }
    }
}