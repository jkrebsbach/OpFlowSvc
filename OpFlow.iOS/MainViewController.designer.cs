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
    [Register ("MainViewController")]
    partial class MainViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnHome { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnHomeText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSchedule { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnScheduleText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSearch { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSearchText { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView MainContainerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwHome { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwSchedule { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwSearch { get; set; }

        [Action ("btnHome_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnHome_Click (UIKit.UIButton sender);

        [Action ("btnSchedule_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnSchedule_Click (UIKit.UIButton sender);

        [Action ("btnSearch_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnSearch_Click (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnHome != null) {
                btnHome.Dispose ();
                btnHome = null;
            }

            if (btnHomeText != null) {
                btnHomeText.Dispose ();
                btnHomeText = null;
            }

            if (btnSchedule != null) {
                btnSchedule.Dispose ();
                btnSchedule = null;
            }

            if (btnScheduleText != null) {
                btnScheduleText.Dispose ();
                btnScheduleText = null;
            }

            if (btnSearch != null) {
                btnSearch.Dispose ();
                btnSearch = null;
            }

            if (btnSearchText != null) {
                btnSearchText.Dispose ();
                btnSearchText = null;
            }

            if (MainContainerView != null) {
                MainContainerView.Dispose ();
                MainContainerView = null;
            }

            if (vwHome != null) {
                vwHome.Dispose ();
                vwHome = null;
            }

            if (vwSchedule != null) {
                vwSchedule.Dispose ();
                vwSchedule = null;
            }

            if (vwSearch != null) {
                vwSearch.Dispose ();
                vwSearch = null;
            }
        }
    }
}