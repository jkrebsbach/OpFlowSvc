using Foundation;
using OpFlow.Mobile;
using System;
using System.IO;
using System.Threading.Tasks;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ImageSetupViewController : UIViewController
    {
        public ImageSetupViewController (IntPtr handle) : base (handle)
        {
            // done button click should fire this:
            //await UploadImage(evt.EditedImage);
        }

        private async Task UploadImage(UIImage sourceImage)
        {

            AppSettings.PriorScreen = AppSettings.CurrentScreen;
            var flowId = AppSettings.CurrentFlow ?? 0;
            await FlowUtil.UploadFlowImage(flowId, AppSettings.CurrentImage);

            // Navigate to flow detail page
        }
    }
}