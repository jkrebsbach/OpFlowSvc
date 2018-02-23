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
    [Register ("NewCommunicationViewController")]
    partial class NewCommunicationViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISearchBar searchUser { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView UserSelectTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (searchUser != null) {
                searchUser.Dispose ();
                searchUser = null;
            }

            if (UserSelectTableView != null) {
                UserSelectTableView.Dispose ();
                UserSelectTableView = null;
            }
        }
    }
}