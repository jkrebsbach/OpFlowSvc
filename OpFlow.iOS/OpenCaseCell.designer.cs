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
    [Register ("OpenCaseCell")]
    partial class OpenCaseCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblFlowStep { get; set; }

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
        UIKit.UILabel lblSurgeryTime { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblFlowStep != null) {
                lblFlowStep.Dispose ();
                lblFlowStep = null;
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

            if (lblSurgeryTime != null) {
                lblSurgeryTime.Dispose ();
                lblSurgeryTime = null;
            }
        }
    }
}