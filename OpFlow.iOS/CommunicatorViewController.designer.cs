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
    [Register ("CommunicatorViewController")]
    partial class CommunicatorViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView PrivateCommunicatorTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView SurgeryCommunicatorTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch swtCaseFilter { get; set; }

        [Action ("swtCaseFilter_Changed:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void swtCaseFilter_Changed (UIKit.UISwitch sender);

        void ReleaseDesignerOutlets ()
        {
            if (PrivateCommunicatorTableView != null) {
                PrivateCommunicatorTableView.Dispose ();
                PrivateCommunicatorTableView = null;
            }

            if (SurgeryCommunicatorTableView != null) {
                SurgeryCommunicatorTableView.Dispose ();
                SurgeryCommunicatorTableView = null;
            }

            if (swtCaseFilter != null) {
                swtCaseFilter.Dispose ();
                swtCaseFilter = null;
            }
        }
    }
}