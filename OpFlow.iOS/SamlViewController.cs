using Foundation;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using System;
using System.Threading.Tasks;
using UIKit;
using WebKit;

namespace OpFlow.iOS
{
    public partial class SamlViewController : OpFlowViewController, IWKNavigationDelegate
    {
        private UIActivityIndicatorView _activity;

        public SamlViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            _activity = new UIActivityIndicatorView()
            {
                Center = this.View.Center,
                HidesWhenStopped = true,

                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge
            };

            _activity.Color = UIColor.Gray;

            View.AddSubview(_activity);

            _activity.StartAnimating();

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.NavigationBar.Hidden = true;

            var basePath = WebUtility.GetWebUrl("saml/idplogin");

            var urlRequest = new NSMutableUrlRequest(
                new NSUrl(basePath));

            urlRequest.HttpMethod = "POST";

            webView.LoadRequest(urlRequest);
            webView.NavigationDelegate = this;

            base.ViewWillAppear(animated);
        }

        [Export("webView:didFinishNavigation:")]
        public async void DidFinishNavigation(WKWebView webView, WKNavigation navigation)
        {
            var navigationUrl = webView.Url.ToString();
            if (navigationUrl.Contains("unc.edu"))
            {
                _activity.StopAnimating();
            } 
            else if (navigationUrl.Contains("authToken="))
            {
                var authToken = navigationUrl.Substring(
                navigationUrl.IndexOf("authToken=", StringComparison.CurrentCulture) + 10);

                await AppSettings.SSOAuthenticateUser(authToken);

                if (AppSettings.UserAuthenticated)
                {
                    await RegisterDevice();

                    // Navigate to schedule page after signon
                    AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;

                    var controller = Storyboard.InstantiateViewController("MainViewController");
                    NavigationController.PushViewController(controller, true);
                }
            }
        }

        private static async Task RegisterDevice()
        {
            var deviceToken = NSUserDefaults.StandardUserDefaults["PushDeviceToken"];

            if (deviceToken != null && deviceToken.ToString() != "")
            {
                var token = deviceToken.ToString();

                await UserUtil.RegisterDevice(token, "apns");
            }
        }

        partial void CancelClicked(UIButton sender)
        {
            NavigationController.PopViewController(true);
        }
    }
}