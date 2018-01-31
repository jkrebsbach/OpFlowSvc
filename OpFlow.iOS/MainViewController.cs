using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class MainViewController : UIViewController, INavigationDelegate
    {
        private ContainerViewController _containerViewController;
        private NSObject _foregroundNotification;

        public MainViewController (IntPtr handle) : base (handle)
        {
        }

        public override void PrepareForSegue(UIStoryboardSegue segue, Foundation.NSObject sender)
        {
            if (segue.Identifier == "embedContainer")
            {
                _containerViewController = segue.DestinationViewController as ContainerViewController;
                _containerViewController?.SetupHost(this);
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (!AppSettings.UserAuthenticated)
            {
                var controller = Storyboard.InstantiateViewController("LoginViewController");
                NavigationController.PushViewController(controller, true);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _foregroundNotification = UIApplication.Notifications.ObserveWillEnterForeground((sender, EventArgs) =>
            {
                if (!AppSettings.UserAuthenticated)
                {
                    var controller = Storyboard.InstantiateViewController("LoginViewController");
                    NavigationController.PushViewController(controller, true);
                }

                _foregroundNotification.Dispose();
            });

            if (!AppSettings.UserAuthenticated)
                return;

            NavigationItem.SetHidesBackButton(true, false);

            NavigationItem.RightBarButtonItem = new UIBarButtonItem()
            {
                Title = AppSettings.CurrentUserTitle
            };
            
            PresentContainerView(AppSettings.FragmentEnum.Schedule);
        }

        public override void ViewDidDisappear(bool animated)
        {
            _foregroundNotification?.Dispose();
        }

        private void NavigateBack(object sender, EventArgs e)
        {
            // switch on current screen
            if (1 == 1)
                PresentContainerView(AppSettings.FragmentEnum.Schedule);
        }

        public void Navigate(AppSettings.FragmentEnum fragmentEnum)
        {
            PresentContainerView(fragmentEnum);
        }

        private async void PresentContainerView(AppSettings.FragmentEnum fragmentEnum)
        {
            if (fragmentEnum == AppSettings.FragmentEnum.Schedule)
            {
                AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;
                Title = "SCHEDULE";

                NavigationItem.LeftBarButtonItem = null;

                await _containerViewController.PresentScheduleViewAsync();
            }
            else if (fragmentEnum == AppSettings.FragmentEnum.CaseNavigate)
            {
                AppSettings.CurrentScreen = AppSettings.FragmentEnum.CaseNavigate;
                Title = "DETAIL";

                NavigationItem.LeftBarButtonItem = SetupCustomBack("Schedule");

                await _containerViewController.PresentNavigateViewAsync();
            }
            else
            {
                AppSettings.CurrentScreen = AppSettings.FragmentEnum.Debrief;
                Title = "DEBRIEF";

                NavigationItem.LeftBarButtonItem = SetupCustomBack("Schedule");

                await _containerViewController.PresentDebriefViewAsync();

            }
        }

        private UIBarButtonItem SetupCustomBack(string backText)
        {
            var backButton = new UIBarButtonItem()
            {
                Title = backText
            };
            backButton.Clicked += NavigateBack;

            return backButton;
        }

        public void Navigate(AppSettings.FragmentEnum fragmentEnum, object payload)
        {
            switch (fragmentEnum)
            {
                case AppSettings.FragmentEnum.Schedule:
                    var surgery = (Surgery) payload;
                    AppSettings.LoadSurgery(surgery.SurgeryID, surgery.PatientID);

                    if (surgery.SurgeryStatus == "O")
                        PresentContainerView(AppSettings.FragmentEnum.Debrief);
                    else
                        PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);

                    break;
                case AppSettings.FragmentEnum.CaseNavigate:
                    PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
                    break;


            }
        }
    }
}