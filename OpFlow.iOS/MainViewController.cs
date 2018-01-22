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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

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
                Title = "SCHEDULE";
                await _containerViewController.PresentScheduleViewAsync();
            }
            else
            {
                Title = "DETAIL";
                await _containerViewController.PresentDetailViewAsync();
            }
        }

        public void Navigate(AppSettings.FragmentEnum fragmentEnum, object payload)
        {
            switch (fragmentEnum)
            {
                case AppSettings.FragmentEnum.Schedule:
                    var surgery = (Surgery) payload;
                    AppSettings.LoadSurgery(surgery.SurgeryID, surgery.PatientID);
                    PresentContainerView(AppSettings.FragmentEnum.CardDetail);
                    break;

            }
        }
    }
}