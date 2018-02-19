using Foundation;
using System;
using UIKit;
using OpFlow.Mobile;

namespace OpFlow.iOS
{
    public partial class HomeViewController : UIViewController
    {
        public HomeViewController(IntPtr handle) : base(handle)
        {
        }

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
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.CardList;

            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnCase_Click(UIButton sender)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.CreateCase;

            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnSearch_Click(UIButton sender)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.SearchCases;

            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnSchedule_Click(UIButton sender)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;

            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }

        partial void btnCommunicator_Click(UIButton sender)
		{

            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Communicator;

            var controller = Storyboard.InstantiateViewController("MainViewController");
            NavigationController.PushViewController(controller, true);
        }
    }
}