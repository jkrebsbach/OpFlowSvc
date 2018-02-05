using Foundation;
using System;
using System.Threading.Tasks;
using CoreGraphics;
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
                Title = AppSettings.CurrentUserTitle,
                TintColor = UIColor.White
            };

            PresentContainerView(AppSettings.FragmentEnum.Schedule);
        }

        public override void ViewDidDisappear(bool animated)
        {
            _foregroundNotification?.Dispose();
        }

        private void NavigateBack(object sender, EventArgs e)
        {
            if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.CaseNavigate)
                PresentContainerView(AppSettings.FragmentEnum.Schedule);
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

        ////https://theconfuzedsourcecode.wordpress.com/2017/03/02/creating-an-identical-custom-navigation-bar-back-button-in-xamarin-ios/
        //private UIBarButtonItem SetupCustomBack(string backText)
        //{
        //    var backBtnImage = UIImage.FromBundle("back.png")
        //        .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);

        //    var backButton = new UIButton(UIButtonType.Custom)
        //    {
        //        HorizontalAlignment =UIControlContentHorizontalAlignment.Left,
        //        TitleEdgeInsets = new UIEdgeInsets(11.5f, 15f, 10f, 0f),
        //        ImageEdgeInsets = new UIEdgeInsets(1f, 8f, 0f, 0f)
        //    };

        //    backButton.SetTitle(backText, UIControlState.Normal);
        //    // use the default blue color in ios back button text
        //    backButton.SetTitleColor(UIColor.FromRGB(0, 129, 249), UIControlState.Normal);
        //    backButton.SetTitleColor(UIColor.LightGray, UIControlState.Highlighted);
        //    backButton.Font = UIFont.FromName("HelveticaNeue", (nfloat)17);
        //    backButton.SetImage(backBtnImage, UIControlState.Normal);
        //    backButton.SizeToFit();

        //    backButton.TouchDown += NavigateBack;
            
        //    backButton.Frame = new CGRect(0, 0, 
        //        UIScreen.MainScreen.Bounds.Width / 4, 
        //        NavigationController.NavigationBar.Frame.Height);

        //    var btnContainer = new UIView(
        //        new CGRect(0, 0, backButton.Frame.Width, backButton.Frame.Height));

        //    btnContainer.AddSubview(backButton);


        //    var backButtonItem = new UIBarButtonItem("", UIBarButtonItemStyle.Plain, null)
        //    {
        //        CustomView = backButton
        //    };

        //    return backButtonItem;
        //}

        public void Navigate(AppSettings.FragmentEnum fragmentEnum, object payload)
        {
            switch (fragmentEnum)
            {
                case AppSettings.FragmentEnum.Schedule:

                    PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
                    break;
                case AppSettings.FragmentEnum.CaseNavigate:
                    PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
                    break;
                case AppSettings.FragmentEnum.Patient:
                    PresentContainerView(AppSettings.FragmentEnum.Patient);
                    break;
                case AppSettings.FragmentEnum.CardDetail:
                    PresentContainerView(AppSettings.FragmentEnum.CardDetail);
                    break;
                case AppSettings.FragmentEnum.FlowDetail:
                    PresentContainerView(AppSettings.FragmentEnum.FlowDetail);
                    break;
                case AppSettings.FragmentEnum.Dashboard:
                    PresentContainerView(AppSettings.FragmentEnum.Dashboard);
                    break;
                case AppSettings.FragmentEnum.Debrief:
                    PresentContainerView(AppSettings.FragmentEnum.Debrief);
                    break;


            }
        }

        private async void PresentContainerView(AppSettings.FragmentEnum fragmentEnum)
        {
            // TODO: Rework back stack implementation to not require this...
            AppSettings.PriorScreen = AppSettings.CurrentScreen;
            AppSettings.CurrentScreen = fragmentEnum;

            UIBarButtonItem customBackButton = null;

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

                case AppSettings.FragmentEnum.FlowDetail:
                    Title = "Flow";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDetailViewAsync();
                    break;

                case AppSettings.FragmentEnum.Dashboard:
                    Title = "Dashboard";
                    customBackButton = SetupCustomBack("Surgery");
                    await _containerViewController.PresentDetailViewAsync();
                    break;
            }


            NavigationItem.LeftBarButtonItem = customBackButton;
        }
    }
}