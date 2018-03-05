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
    [Register ("CardListViewController")]
    partial class CardListViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView CardListTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtBundle { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtProcedure { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (CardListTableView != null) {
                CardListTableView.Dispose ();
                CardListTableView = null;
            }

            if (txtBundle != null) {
                txtBundle.Dispose ();
                txtBundle = null;
            }

            if (txtProcedure != null) {
                txtProcedure.Dispose ();
                txtProcedure = null;
            }
        }
    }
}