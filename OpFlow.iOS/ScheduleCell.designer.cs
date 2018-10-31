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
    [Register ("ScheduleCell")]
    partial class ScheduleCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnReview { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblDuration { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblLocation { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPatientInfo { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblPatientName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblProcedure { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblStartTime { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSurgeryTeam { get; set; }

        [Action ("btnReview_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnReview_Click (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnReview != null) {
                btnReview.Dispose ();
                btnReview = null;
            }

            if (lblDuration != null) {
                lblDuration.Dispose ();
                lblDuration = null;
            }

            if (lblLocation != null) {
                lblLocation.Dispose ();
                lblLocation = null;
            }

            if (lblPatientInfo != null) {
                lblPatientInfo.Dispose ();
                lblPatientInfo = null;
            }

            if (lblPatientName != null) {
                lblPatientName.Dispose ();
                lblPatientName = null;
            }

            if (lblProcedure != null) {
                lblProcedure.Dispose ();
                lblProcedure = null;
            }

            if (lblStartTime != null) {
                lblStartTime.Dispose ();
                lblStartTime = null;
            }

            if (lblSurgeryTeam != null) {
                lblSurgeryTeam.Dispose ();
                lblSurgeryTeam = null;
            }
        }
    }
}