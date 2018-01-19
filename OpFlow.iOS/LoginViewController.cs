using Foundation;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class LoginViewController : UIViewController
    {
        public LoginViewController (IntPtr handle) : base (handle)
        {
        }

        partial void SignOnClick(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("ScheduleViewController");
            NavigationController.PushViewController(controller, true);
        }
    }
}