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
        UIKit.UILabel lblDescription { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPosition { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSurgeonPosition { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblDescription != null) {
                lblDescription.Dispose ();
                lblDescription = null;
            }

            if (lblPosition != null) {
                lblPosition.Dispose ();
                lblPosition = null;
            }

            if (lblSurgeonPosition != null) {
                lblSurgeonPosition.Dispose ();
                lblSurgeonPosition = null;
            }
        }
    }
}