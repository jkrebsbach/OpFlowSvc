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

        partial void SignOnClick(UIButton sender)
        {
            var task = Task.Run(async () =>
            {
                await AppSettings.AuthenticateUser("info@opflowtech.com", "OpFlow1!");

            });

            task.Wait();

            var controller = Storyboard.InstantiateViewController("ScheduleViewController");
            NavigationController.PushViewController(controller, true);
        }
    }
}