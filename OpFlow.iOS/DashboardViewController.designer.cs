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
    [Register ("DashboardViewController")]
    partial class DashboardViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView DashboardTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblEstDuration { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblEstEnd { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblExpDuration { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblStart { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (DashboardTableView != null) {
                DashboardTableView.Dispose ();
                DashboardTableView = null;
            }

            if (lblEstDuration != null) {
                lblEstDuration.Dispose ();
                lblEstDuration = null;
            }

            if (lblEstEnd != null) {
                lblEstEnd.Dispose ();
                lblEstEnd = null;
            }

            if (lblExpDuration != null) {
                lblExpDuration.Dispose ();
                lblExpDuration = null;
            }

            if (lblStart != null) {
                lblStart.Dispose ();
                lblStart = null;
            }
        }
    }
}