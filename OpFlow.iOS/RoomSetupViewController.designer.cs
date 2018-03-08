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
    [Register ("RoomSetupViewController")]
    partial class RoomSetupViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView RoomSearchTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtPosition { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtRoomType { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (RoomSearchTableView != null) {
                RoomSearchTableView.Dispose ();
                RoomSearchTableView = null;
            }

            if (txtPosition != null) {
                txtPosition.Dispose ();
                txtPosition = null;
            }

            if (txtRoomType != null) {
                txtRoomType.Dispose ();
                txtRoomType = null;
            }
        }
    }
}