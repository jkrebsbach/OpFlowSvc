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
    [Register ("DashboardStepCell")]
    partial class DashboardStepCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblDelay { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblEnd { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPhase { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblProcedure { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblStart { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblDelay != null) {
                lblDelay.Dispose ();
                lblDelay = null;
            }

            if (lblEnd != null) {
                lblEnd.Dispose ();
                lblEnd = null;
            }

            if (lblPhase != null) {
                lblPhase.Dispose ();
                lblPhase = null;
            }

            if (lblProcedure != null) {
                lblProcedure.Dispose ();
                lblProcedure = null;
            }

            if (lblStart != null) {
                lblStart.Dispose ();
                lblStart = null;
            }
        }
    }
}