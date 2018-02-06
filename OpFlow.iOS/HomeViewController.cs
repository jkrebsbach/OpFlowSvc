using Foundation;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class HomeViewController : UIViewController
    {
        partial void btnCommunity_Click(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnAnalytics_Click(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnCard_Click(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnCase_Click(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnSearch_Click(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnSchedule_Click(UIButton sender)
        {
            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        public HomeViewController (IntPtr handle) : base (handle)
        {
        }
    }
}