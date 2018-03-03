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
        UIKit.UIButton btnFriday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnMonday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnNext { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnPrev { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSaturday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSunday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnThursday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnTuesday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnWednesday { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UILabel lblCurrentWeek { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIPickerView pickerRoom { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView ScheduleTableView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UISwitch switchSurgeon { get; set; }

        [Action ("btnNextClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnNextClicked (UIKit.UIButton sender);

        [Action ("btnPrevClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void btnPrevClicked (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnFriday != null) {
                btnFriday.Dispose ();
                btnFriday = null;
            }

            if (btnMonday != null) {
                btnMonday.Dispose ();
                btnMonday = null;
            }

            if (btnNext != null) {
                btnNext.Dispose ();
                btnNext = null;
            }

            if (btnPrev != null) {
                btnPrev.Dispose ();
                btnPrev = null;
            }

            if (btnSaturday != null) {
                btnSaturday.Dispose ();
                btnSaturday = null;
            }

            if (btnSunday != null) {
                btnSunday.Dispose ();
                btnSunday = null;
            }

            if (btnThursday != null) {
                btnThursday.Dispose ();
                btnThursday = null;
            }

            if (btnTuesday != null) {
                btnTuesday.Dispose ();
                btnTuesday = null;
            }

            if (btnWednesday != null) {
                btnWednesday.Dispose ();
                btnWednesday = null;
            }

            if (lblCurrentWeek != null) {
                lblCurrentWeek.Dispose ();
                lblCurrentWeek = null;
            }

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