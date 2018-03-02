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
    [Register ("RoomSearchResultCell")]
    partial class RoomSearchResultCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPosition { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblRoom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSetupName { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblPosition != null) {
                lblPosition.Dispose ();
                lblPosition = null;
            }

            if (lblRoom != null) {
                lblRoom.Dispose ();
                lblRoom = null;
            }

            if (lblSetupName != null) {
                lblSetupName.Dispose ();
                lblSetupName = null;
            }
        }
    }
}