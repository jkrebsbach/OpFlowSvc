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
    [Register ("SurgeryCommunicatorCell")]
    partial class SurgeryCommunicatorCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIImageView ivCommunicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblCommunicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblInitials { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ivCommunicator != null) {
                ivCommunicator.Dispose ();
                ivCommunicator = null;
            }

            if (lblCommunicator != null) {
                lblCommunicator.Dispose ();
                lblCommunicator = null;
            }

            if (lblInitials != null) {
                lblInitials.Dispose ();
                lblInitials = null;
            }
        }
    }
}