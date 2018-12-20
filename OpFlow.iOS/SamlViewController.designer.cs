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
    [Register ("SamlViewController")]
    partial class SamlViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnCancel { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        WebKit.WKWebView webView { get; set; }

        [Action ("CancelClicked:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void CancelClicked (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnCancel != null) {
                btnCancel.Dispose ();
                btnCancel = null;
            }

            if (webView != null) {
                webView.Dispose ();
                webView = null;
            }
        }
    }
}