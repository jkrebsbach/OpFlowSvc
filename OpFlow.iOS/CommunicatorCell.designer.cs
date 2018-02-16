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
    [Register ("CommunicatorCell")]
    partial class CommunicatorCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblMessageGroup { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblRecentMessage { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblMessageGroup != null) {
                lblMessageGroup.Dispose ();
                lblMessageGroup = null;
            }

            if (lblRecentMessage != null) {
                lblRecentMessage.Dispose ();
                lblRecentMessage = null;
            }
        }
    }
}