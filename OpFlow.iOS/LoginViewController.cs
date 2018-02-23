using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class LoginViewController : OpFlowViewController
    {
        public LoginViewController (IntPtr handle) : base (handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            //  sizing issues moving windows up and down - just keep it all down
            //NavigationItem.SetHidesBackButton(true, false);
            //NavigationController.NavigationBar.Hidden = true;
        }

        async partial void SignOnClick(UIButton sender)
        {
            try
            {
                ShowPleaseWait("Authenticating...");
                await AppSettings.AuthenticateUser("info@opflowtech.com", "OpFlow1!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                    
                ShowDialog("Error", "Problems authenticating user");
            }

            HidePleaseWait();

            if (AppSettings.UserAuthenticated)
            {
                // Navigate to schedule page after signon
                AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;

                var controller = Storyboard.InstantiateViewController("MainViewController");
                NavigationController.PushViewController(controller, true);

            }
        }
    }
}