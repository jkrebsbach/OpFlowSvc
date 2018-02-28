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
    [Register ("SearchViewController")]
    partial class SearchViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView CaseSearchTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtCase { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtCRNA { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtDate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtPA { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtPatient { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtRep { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtRoom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtSurgeon { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CaseSearchTableView != null) {
                CaseSearchTableView.Dispose ();
                CaseSearchTableView = null;
            }

            if (txtCase != null) {
                txtCase.Dispose ();
                txtCase = null;
            }

            if (txtCRNA != null) {
                txtCRNA.Dispose ();
                txtCRNA = null;
            }

            if (txtDate != null) {
                txtDate.Dispose ();
                txtDate = null;
            }

            if (txtPA != null) {
                txtPA.Dispose ();
                txtPA = null;
            }

            if (txtPatient != null) {
                txtPatient.Dispose ();
                txtPatient = null;
            }

            if (txtRep != null) {
                txtRep.Dispose ();
                txtRep = null;
            }

            if (txtRoom != null) {
                txtRoom.Dispose ();
                txtRoom = null;
            }

            if (txtSurgeon != null) {
                txtSurgeon.Dispose ();
                txtSurgeon = null;
            }
        }
    }
}