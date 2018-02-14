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
        UIKit.UIButton ivCommunicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel txtCommunicator { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (ivCommunicator != null) {
                ivCommunicator.Dispose ();
                ivCommunicator = null;
            }

            if (txtCommunicator != null) {
                txtCommunicator.Dispose ();
                txtCommunicator = null;
            }
        }
    }
}