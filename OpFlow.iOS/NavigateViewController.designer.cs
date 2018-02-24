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
    [Register ("NavigateViewController")]
    partial class NavigateViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnCard { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnCommunicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnDashboard { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnDebrief { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnPatient { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSendMessage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView CommunicatorTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblAnes { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblAnesName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblCirculator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblCirculatorName { get; set; }

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
        UIKit.UILabel lblRep { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblRepName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblScrub { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblScrubName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSurgeon { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSurgeonName { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblSurgeryTime { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtCommunicator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwAnes { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwCirculator { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwRep { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwScrub { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView vwSurgeon { get; set; }

        [Action ("btnCard_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnCard_Click (UIKit.UIButton sender);

        [Action ("btnCommunicator_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnCommunicator_Click (UIKit.UIButton sender);

        [Action ("btnDashboard_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnDashboard_Click (UIKit.UIButton sender);

        [Action ("btnDebrief_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnDebrief_Click (UIKit.UIButton sender);

        [Action ("btnFlow_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnFlow_Click (UIKit.UIButton sender);

        [Action ("btnPatient_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnPatient_Click (UIKit.UIButton sender);

        [Action ("btnSendMessage_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnSendMessage_Click (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnCard != null) {
                btnCard.Dispose ();
                btnCard = null;
            }

            if (btnCommunicator != null) {
                btnCommunicator.Dispose ();
                btnCommunicator = null;
            }

            if (btnDashboard != null) {
                btnDashboard.Dispose ();
                btnDashboard = null;
            }

            if (btnDebrief != null) {
                btnDebrief.Dispose ();
                btnDebrief = null;
            }

            if (btnPatient != null) {
                btnPatient.Dispose ();
                btnPatient = null;
            }

            if (btnSendMessage != null) {
                btnSendMessage.Dispose ();
                btnSendMessage = null;
            }

            if (CommunicatorTableView != null) {
                CommunicatorTableView.Dispose ();
                CommunicatorTableView = null;
            }

            if (lblAnes != null) {
                lblAnes.Dispose ();
                lblAnes = null;
            }

            if (lblAnesName != null) {
                lblAnesName.Dispose ();
                lblAnesName = null;
            }

            if (lblCirculator != null) {
                lblCirculator.Dispose ();
                lblCirculator = null;
            }

            if (lblCirculatorName != null) {
                lblCirculatorName.Dispose ();
                lblCirculatorName = null;
            }

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

            if (lblRep != null) {
                lblRep.Dispose ();
                lblRep = null;
            }

            if (lblRepName != null) {
                lblRepName.Dispose ();
                lblRepName = null;
            }

            if (lblScrub != null) {
                lblScrub.Dispose ();
                lblScrub = null;
            }

            if (lblScrubName != null) {
                lblScrubName.Dispose ();
                lblScrubName = null;
            }

            if (lblSurgeon != null) {
                lblSurgeon.Dispose ();
                lblSurgeon = null;
            }

            if (lblSurgeonName != null) {
                lblSurgeonName.Dispose ();
                lblSurgeonName = null;
            }

            if (lblSurgeryTime != null) {
                lblSurgeryTime.Dispose ();
                lblSurgeryTime = null;
            }

            if (txtCommunicator != null) {
                txtCommunicator.Dispose ();
                txtCommunicator = null;
            }

            if (vwAnes != null) {
                vwAnes.Dispose ();
                vwAnes = null;
            }

            if (vwCirculator != null) {
                vwCirculator.Dispose ();
                vwCirculator = null;
            }

            if (vwRep != null) {
                vwRep.Dispose ();
                vwRep = null;
            }

            if (vwScrub != null) {
                vwScrub.Dispose ();
                vwScrub = null;
            }

            if (vwSurgeon != null) {
                vwSurgeon.Dispose ();
                vwSurgeon = null;
            }
        }
    }
}