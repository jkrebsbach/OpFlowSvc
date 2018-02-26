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
    [Register ("CreateCaseViewController")]
    partial class CreateCaseViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtBundle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtCaseNbr { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtCptCode { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtDefaultCard { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtDefaultFlow { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtDefaultRoom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtGender { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtInitials { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtPatientId { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtSpecialty { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtSurgeon { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (txtBundle != null) {
                txtBundle.Dispose ();
                txtBundle = null;
            }

            if (txtCaseNbr != null) {
                txtCaseNbr.Dispose ();
                txtCaseNbr = null;
            }

            if (txtCptCode != null) {
                txtCptCode.Dispose ();
                txtCptCode = null;
            }

            if (txtDefaultCard != null) {
                txtDefaultCard.Dispose ();
                txtDefaultCard = null;
            }

            if (txtDefaultFlow != null) {
                txtDefaultFlow.Dispose ();
                txtDefaultFlow = null;
            }

            if (txtDefaultRoom != null) {
                txtDefaultRoom.Dispose ();
                txtDefaultRoom = null;
            }

            if (txtGender != null) {
                txtGender.Dispose ();
                txtGender = null;
            }

            if (txtInitials != null) {
                txtInitials.Dispose ();
                txtInitials = null;
            }

            if (txtPatientId != null) {
                txtPatientId.Dispose ();
                txtPatientId = null;
            }

            if (txtSpecialty != null) {
                txtSpecialty.Dispose ();
                txtSpecialty = null;
            }

            if (txtSurgeon != null) {
                txtSurgeon.Dispose ();
                txtSurgeon = null;
            }
        }
    }
}