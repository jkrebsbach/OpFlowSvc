using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class LoginViewController : UIViewController
    {
        public LoginViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            NavigationItem.SetHidesBackButton(true, false);
        }

        partial void SignOnClick(UIButton sender)
        {
            var task = Task.Run(async () =>
            {
                await AppSettings.AuthenticateUser("info@opflowtech.com", "OpFlow1!");

            });

            task.Wait();

            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }
    }
}