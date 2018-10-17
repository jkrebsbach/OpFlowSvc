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
        UIKit.UIButton btnAssign { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView CaseSearchTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtBeginDate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtEndDate { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtRoom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtSpecialty { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtSurgeon { get; set; }

        [Action ("btnAssign_Click:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnAssign_Click (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnAssign != null) {
                btnAssign.Dispose ();
                btnAssign = null;
            }

            if (CaseSearchTableView != null) {
                CaseSearchTableView.Dispose ();
                CaseSearchTableView = null;
            }

            if (txtBeginDate != null) {
                txtBeginDate.Dispose ();
                txtBeginDate = null;
            }

            if (txtEndDate != null) {
                txtEndDate.Dispose ();
                txtEndDate = null;
            }

            if (txtRoom != null) {
                txtRoom.Dispose ();
                txtRoom = null;
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