// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;
using UIKit;

namespace OpFlow.iOS
{
    [Register ("ImageSetupViewController")]
    partial class ImageSetupViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnAccept { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtRole { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtStep { get; set; }

        [Action ("btnAccept_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnAccept_Click (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnAccept != null) {
                btnAccept.Dispose ();
                btnAccept = null;
            }

            if (txtRole != null) {
                txtRole.Dispose ();
                txtRole = null;
            }

            if (txtStep != null) {
                txtStep.Dispose ();
                txtStep = null;
            }
        }
    }
}