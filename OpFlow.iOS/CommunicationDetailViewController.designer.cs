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
    [Register ("CommunicationDetailViewController")]
    partial class CommunicationDetailViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSendMessage { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtMessage { get; set; }

        void ReleaseDesignerOutlets ()
        {
            if (btnSendMessage != null) {
                btnSendMessage.Dispose ();
                btnSendMessage = null;
            }

            if (txtMessage != null) {
                txtMessage.Dispose ();
                txtMessage = null;
            }
        }
    }
}