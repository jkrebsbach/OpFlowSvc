using Foundation;
using System;
using System.Drawing;
using System.Threading.Tasks;
using CoreGraphics;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;
using CoreAnimation;

namespace OpFlow.iOS
{
    public partial class MainViewController : UIViewController, INavigationDelegate
    {
        private ContainerViewController _containerViewController;
        private NSObject _foregroundNotification;

        public event EventHandler DoneEventFired;

        public MainViewController (IntPtr handle) : base (handle)
        {
        }

        partial void btnHome_Click(UIKit.UIButton sender)
        {
            NavigateHome();
        }

        partial void btnSearch_Click(UIKit.UIButton sender)
        {
            PresentContainerView(AppSettings.FragmentEnum.SearchCases);
        }

        partial void btnSchedule_Click(UIKit.UIButton sender)
        {
            PresentContainerView(AppSettings.FragmentEnum.Schedule);
        }

        private void NavigateHome()
        {
            var controller = Storyboard.InstantiateViewController("HomeViewController");
            NavigationController.PushViewController(controller, true);
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
                Title = AppSettings.CurrentUserTitle,
                TintColor = UIColor.Black
            };


            PresentContainerView(AppSettings.CurrentScreen);
        }
        
        public override void ViewDidDisappear(bool animated)
        {
            _foregroundNotification?.Dispose();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            var topBorder = new CALayer
            {
                Frame = new CGRect(0f, 0f, vwSearch.Frame.Width, 2.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            vwSearch.Layer.AddSublayer(topBorder);

            topBorder = new CALayer
            {
                Frame = new CGRect(0f, 0f, vwHome.Frame.Width, 2.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            vwHome.Layer.AddSublayer(topBorder);

            topBorder = new CALayer
            {
                Frame = new CGRect(0f, 0f, vwSchedule.Frame.Width, 2.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            vwSchedule.Layer.AddSublayer(topBorder);

            NavigationController.NavigationBar.Hidden = false;
        }

        private void NavigateBack(object sender, EventArgs e)
        {
            // TODO: Need to think through various navigation channels here....
            // TODO: Some of these actions should be right button cancel...
            if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.CaseNavigate)
                PresentContainerView(AppSettings.FragmentEnum.Schedule);
            else if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.Communicator)
                PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
            else if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.CreateCase)
                NavigateHome();
            else if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.SearchCases)
                NavigateHome();
            else
                PresentContainerView(AppSettings.PriorScreen);
        }

        private UIBarButtonItem SetupCustomBack(string backText)
        {
            var containView = new UIView(new CGRect(0, 0, 102, 40));
            var label = new UIButton(new CGRect(12, 0, 90, 40));

            label.SetTitle(backText, UIControlState.Normal);
            label.SetTitleColor(label.TintColor, UIControlState.Normal);
            label.TitleLabel.TextAlignment = UITextAlignment.Left;
            
            containView.AddSubview(label);

            var imageView = new UIButton(new CGRect(0, 10, 20, 20));
            imageView.SetImage(UIImage.FromBundle("back.png"), UIControlState.Normal);
            imageView.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            containView.AddSubview(imageView);

            var buttonItem = new UIBarButtonItem(containView)
            {
                Target = this
            };

            buttonItem.Clicked += NavigateBack;
            label.TouchDown += NavigateBack;
            imageView.TouchDown += NavigateBack;

            return buttonItem;
        }

        private UIBarButtonItem SetupCustomEdit(CustomButtonType buttonType)
        {
            UIButton uiButton;
            EventHandler navigationAction = NavBarEdit;

            switch (buttonType)
            {
                case CustomButtonType.Compose:
                    uiButton = new UIButton(new CGRect(0, 0, 40, 40));
                    uiButton.SetImage(UIImage.FromBundle("Navigation_Compose.png"), UIControlState.Normal);
                    uiButton.ImageView.ContentMode = UIViewContentMode.ScaleAspectFit;
                    break;
                case CustomButtonType.Cancel:
                    uiButton = new UIButton(new CGRect(0, 0, 75, 40));
                    uiButton.SetTitle("Cancel", UIControlState.Normal);
                    uiButton.SetTitleColor(uiButton.TintColor, UIControlState.Normal);

                    navigationAction = NavigateBack;
                    break;
                default:
                    uiButton = new UIButton(new CGRect(0, 0, 75, 40));
                    uiButton.SetTitle("Done", UIControlState.Normal);
                    uiButton.SetTitleColor(uiButton.TintColor, UIControlState.Normal);
                    break;
            }

            var containView = new UIView(new CGRect(0, 0, uiButton.Frame.Width, uiButton.Frame.Height));
            containView.AddSubview(uiButton);

            var buttonItem = new UIBarButtonItem(containView)
            {
                Target = this
            };

            buttonItem.Clicked += navigationAction;
            uiButton.TouchDown += navigationAction;

            return buttonItem;
        }

        private enum CustomButtonType
        {
            Compose,
            Cancel,
            Done
        }



        // Alternate implementation of custom nav bar at:
        ////https://theconfuzedsourcecode.wordpress.com/2017/03/02/creating-an-identical-custom-navigation-bar-back-button-in-xamarin-ios/

        public async void PresentContainerView(AppSettings.FragmentEnum fragmentEnum)
        {
            // TODO: Rework back stack implementation to not require this...
            AppSettings.PriorScreen = AppSettings.CurrentScreen;
            AppSettings.CurrentScreen = fragmentEnum;

            UIBarButtonItem customBackButton = null;
            var rightButton = new UIBarButtonItem()
                {
                    Title = AppSettings.CurrentUserTitle,
                    TintColor = UIColor.Black
                };

            switch (fragmentEnum)
            {
                case AppSettings.FragmentEnum.Schedule:
                    Title = "Schedule";
                    await _containerViewController.PresentScheduleViewAsync();
                    break;

                case AppSettings.FragmentEnum.CaseNavigate:
                    Title = "Surgery";
                    customBackButton = SetupCustomBack("Schedule");
                    await _containerViewController.PresentNavigateViewAsync();
                    break;

                case AppSettings.FragmentEnum.Debrief:
                    Title = "Debrief";
                    customBackButton = SetupCustomBack("Schedule");
                    await _containerViewController.PresentDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.Patient:
                    Title = "Patient";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.CardDetail:
                    Title = "Card";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.CardList:
                    Title = "Card List";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentCardListViewAsync();
                    break;

                case AppSettings.FragmentEnum.FlowDetail:
                    Title = "Flow";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.Dashboard:
                    Title = "Dashboard";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDashboardViewAsync();
                    break;

                case AppSettings.FragmentEnum.Room:
                    Title = "Room";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.SearchCases:
                    Title = "Search Cases";
                    customBackButton = null; // Cancel action, not back
                    await _containerViewController.PresentSearchCaseViewAsync();

                    rightButton = SetupCustomEdit(CustomButtonType.Cancel);

                    break;

                case AppSettings.FragmentEnum.CreateCase:
                    Title = "New Surgery";
                    customBackButton = null; // Cancel action, not back
                    await _containerViewController.PresentCreateCaseViewAsync();

                    rightButton = SetupCustomEdit(CustomButtonType.Cancel);
                    break;

                case AppSettings.FragmentEnum.Communicator:
                    Title = "Communicator";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentCommunicatorViewAsync();

                    rightButton = SetupCustomEdit(CustomButtonType.Compose);
                    break;

                case AppSettings.FragmentEnum.CommunicationDetail:
                    Title = AppSettings.CurrentMessagingGroup.CommunicationTargetName;
                    customBackButton = SetupCustomBack("Communicator");

                    await _containerViewController.PresentCommunicatorDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.NewCommunicationSetup:
                    Title = "Select User";
                    customBackButton = null; // Cancel action, not back

                    await _containerViewController.PresentCaseGroupEditViewAsync();

                    rightButton = SetupCustomEdit(CustomButtonType.Cancel);
                    break;

            }

            NavigationItem.LeftBarButtonItem = customBackButton;
            NavigationItem.RightBarButtonItems = new[] { rightButton };
        }

        private void NavBarEdit(object sender, EventArgs e)
        {
            if (DoneEventFired != null)
            {
                DoneEventFired(sender, e);
                return;
            }

            PresentContainerView(AppSettings.FragmentEnum.NewCommunicationSetup);
        }
    }
}