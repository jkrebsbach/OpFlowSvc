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
    [Register ("LoginViewController")]
    partial class LoginViewController
    {
        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSaml { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UIButton btnSignOn { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtPassword { get; set; }

        [Outlet]
        [GeneratedCode ("iOS Designer", "1.0")]
        UIKit.UITextField txtUsername { get; set; }

        [Action ("SamlClick:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SamlClick (UIKit.UIButton sender);

        [Action ("SignOnClick:")]
        [GeneratedCode ("iOS Designer", "1.0")]
        partial void SignOnClick (UIKit.UIButton sender);

        void ReleaseDesignerOutlets ()
        {
            if (btnSaml != null) {
                btnSaml.Dispose ();
                btnSaml = null;
            }

            if (btnSignOn != null) {
                btnSignOn.Dispose ();
                btnSignOn = null;
            }

            if (txtPassword != null) {
                txtPassword.Dispose ();
                txtPassword = null;
            }

            if (txtUsername != null) {
                txtUsername.Dispose ();
                txtUsername = null;
            }
        }
    }
}