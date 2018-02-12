using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class OpFlowViewController : UIViewController
    {
        protected OpFlowViewController(IntPtr handle) : base(handle)
        {
        }

        public void ShowDialog(string title, string message)
        {
            var alertDialog =
                UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alertDialog.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            PresentViewController(alertDialog, true, null);
        }

    }
}