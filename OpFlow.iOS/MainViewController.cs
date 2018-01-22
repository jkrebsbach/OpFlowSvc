using Foundation;
using System;
using System.Threading.Tasks;
using UIKit;

namespace OpFlow.iOS
{
    public partial class MainViewController : UIViewController
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
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            PresentContainerView();
        }

        private async void PresentContainerView()
        {
            if (1 == 1)
                await _containerViewController.PresentScheduleViewAsync();
            else
            {
                await _containerViewController.PresentDetailViewAsync();
            }
        }
    }
}