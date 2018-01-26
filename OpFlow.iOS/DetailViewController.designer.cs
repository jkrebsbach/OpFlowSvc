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
    [Register ("DetailViewController")]
    partial class DetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITableView DetailTableView { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (DetailTableView != null) {
                DetailTableView.Dispose ();
                DetailTableView = null;
            }
        }
    }
}