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
    [Register ("CardTableCell")]
    partial class CardTableCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblProcedureCost { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblProcedureDetail { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblProcedureName { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblProcedureCost != null) {
                lblProcedureCost.Dispose ();
                lblProcedureCost = null;
            }

            if (lblProcedureDetail != null) {
                lblProcedureDetail.Dispose ();
                lblProcedureDetail = null;
            }

            if (lblProcedureName != null) {
                lblProcedureName.Dispose ();
                lblProcedureName = null;
            }
        }
    }
}