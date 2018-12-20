using Foundation;
using OpFlow.iOS.Delegates;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SamlViewController : OpFlowViewController
    {
        public SamlViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.NavigationBar.Hidden = true;

            var urlRequest = new NSMutableUrlRequest(
                new NSUrl("https://opflowservice.azurewebsites.net/saml/idplogin"));

            urlRequest.HttpMethod = "POST";

            webView.LoadRequest(urlRequest);

            base.ViewWillAppear(animated);
        }

        partial void CancelClicked(UIButton sender)
        {
            NavigationController.PopViewController(true);
        }
    }
}