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
    [Register ("ScheduleViewController")]
    partial class ScheduleViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView pickerRoom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ScheduleTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch switchSurgeon { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (pickerRoom != null) {
                pickerRoom.Dispose ();
                pickerRoom = null;
            }

            if (ScheduleTableView != null) {
                ScheduleTableView.Dispose ();
                ScheduleTableView = null;
            }

            if (switchSurgeon != null) {
                switchSurgeon.Dispose ();
                switchSurgeon = null;
            }
        }
    }
}