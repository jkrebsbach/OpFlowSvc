using Foundation;
using System;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SearchViewController : OpFlowViewController
    {
        public SearchViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            //txtMessage.SetupDoneStyleTextField();
        }
    }
}