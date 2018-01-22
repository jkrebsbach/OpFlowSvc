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
    [Register ("MainViewController")]
    partial class MainViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIView MainContainerView { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UINavigationBar MainNavBar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBar MainTabBar { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITabBarItem TabBarSchedule { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (MainContainerView != null) {
                MainContainerView.Dispose ();
                MainContainerView = null;
            }

            if (MainNavBar != null) {
                MainNavBar.Dispose ();
                MainNavBar = null;
            }

            if (MainTabBar != null) {
                MainTabBar.Dispose ();
                MainTabBar = null;
            }

            if (TabBarSchedule != null) {
                TabBarSchedule.Dispose ();
                TabBarSchedule = null;
            }
        }
    }
}