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
        UIKit.UILabel lblDetails { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSummary { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblDetails != null) {
                lblDetails.Dispose ();
                lblDetails = null;
            }

            if (lblSummary != null) {
                lblSummary.Dispose ();
                lblSummary = null;
            }
        }
    }
}