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
    [Register ("CaseSearchResultCell")]
    partial class CaseSearchResultCell
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblCaseOverview { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblCaseProcedure { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch switchSelectCase { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (lblCaseOverview != null) {
                lblCaseOverview.Dispose ();
                lblCaseOverview = null;
            }

            if (lblCaseProcedure != null) {
                lblCaseProcedure.Dispose ();
                lblCaseProcedure = null;
            }

            if (switchSelectCase != null) {
                switchSelectCase.Dispose ();
                switchSelectCase = null;
            }
        }
    }
}