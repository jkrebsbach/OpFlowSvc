using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class OpFlowViewController : UIViewController, INavigationTargetDelegate
    {
        private NSObject keyboardUp = null;
        private NSObject keyboardDown = null;

        protected OpFlowViewController(IntPtr handle) : base(handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            keyboardUp = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidShowNotification, KeyBoardUpNotification);
            keyboardDown = NSNotificationCenter.DefaultCenter.AddObserver(UIKeyboard.DidHideNotification, KeyBoardDownNotification);
        }

        public override void ViewDidDisappear(bool animated)
        {
            if (keyboardUp != null) NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardUp);
            if (keyboardDown != null) NSNotificationCenter.DefaultCenter.RemoveObserver(keyboardDown);
        }

        protected async Task ExecuteAsyncWebRequest(Task webTask, string message = "Thinking...")
        {
            UIAlertController pleaseWait = null;

            try
            {
                pleaseWait = ShowPleaseWait(message);
                await webTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ShowDialog("Error", "Problems communicating with server");
            }

            if (pleaseWait != null)
            {
                if (!pleaseWait.IsViewLoaded)
                    await Task.Delay(500);

                HidePleaseWait(pleaseWait);
            }
        }

        protected async Task<T> ExecuteAsyncWebRequest<T>(Task<T> webTask, string message = "Thinking...")
        {
            var result = default(T);
            UIAlertController pleaseWait = null;

            try
            {
                pleaseWait = ShowPleaseWait(message);
                result = await webTask;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

                ShowDialog("Error", "Problems communicating with server");
            }

            if (pleaseWait != null)
            {
                if (!pleaseWait.IsViewLoaded)
                    await Task.Delay(500);

                HidePleaseWait(pleaseWait);
            }
            return result;
        }

        // http://www.gooorack.com/2013/08/28/xamarin-moving-the-view-on-keyboard-show/
        private nfloat scrollAmount = 0f; // amount to scroll

        private bool moveViewUp = false; // Direction we are moving
        private UIView activeView; // Controller that activated the keyboard

        private void KeyBoardUpNotification(NSNotification notification)
        {
            // some keyboards fire multiple events
            //if (moveViewUp)
            //    return;

            // get the keyboard size
            var r = UIKeyboard.BoundsFromNotification(notification);

            // Find what opened the keyboard
            // Bottom of the controller = initial position + height + offset      
            activeView = RecurseSubviews(this.View);

            if (activeView == null)
                return; // This message is for a different view container...

            var bottom = activeView.Frame.Y + activeView.Frame.Height;
            var parent = activeView.Superview;
            while (parent != null && parent != this.View)
            {
                bottom += parent.Frame.Y - parent.Bounds.Y;
                parent = parent.Superview;
            }
            
            bottom += this.View.Frame.Y - this.View.Bounds.Y;

            // Calculate how far we need to scroll
            scrollAmount = (r.Height - (View.Frame.Size.Height - bottom));

            var navBarHeight = NavigationController?.NavigationBar?.Frame.Height ?? 0;
            scrollAmount -= navBarHeight;

            // Perform the scrolling
            if (scrollAmount > 0)
            {
                moveViewUp = true;
                ScrollTheView(moveViewUp);
            }
        }

        private UIView RecurseSubviews(UIView sourceView)
        {
            foreach (var view in sourceView.Subviews)
            {
                var responder = RecurseSubviews(view);

                if (responder != null) return responder;
            }

            return sourceView.IsFirstResponder ? sourceView : null;
        }

        private void KeyBoardDownNotification(NSNotification notification)
        {
            if (moveViewUp)
            {
                ScrollTheView(false);
                moveViewUp = false;
            }
        }

        private void ScrollTheView(bool move)
        {
            // scroll the view up or down
            UIView.BeginAnimations(string.Empty, System.IntPtr.Zero);
            UIView.SetAnimationDuration(0.3);

            var frame = View.Frame;

            if (move)
            {
                frame.Y -= scrollAmount;
            }
            else
            {
                //frame.Y += scrollAmount;
                frame.Y = 0;
                scrollAmount = 0;
            }

            View.Frame = frame;
            UIView.CommitAnimations();
        }

        public UIAlertController ShowDialog(string title, string message)
        {
            var alertDialog =
                UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alertDialog.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

            PresentViewController(alertDialog, true, null);
            return alertDialog;
        }

        public UIAlertController ShowPleaseWait(string text = "Please wait")
        {
            var alertDialog =
                UIAlertController.Create(null, text, UIAlertControllerStyle.Alert);
            
            PresentViewController(alertDialog, true, null);
            return alertDialog;
        }

        public void HidePleaseWait(UIAlertController alertDialog)
        {
            InvokeOnMainThread(() =>
            {


                alertDialog.DismissViewController(true, null);
            });
        }


        public void SetupDoneStyleTextBox(UITextView textView)
        {
            textView.Editable = false;

            textView.ReturnKeyType = UIReturnKeyType.Done;

            textView.ShouldChangeText = (text, range, replacementString) =>
            {
                if (replacementString.Equals("\n"))
                {
                    textView.EndEditing(true);
                    return false;
                }
                else
                {
                    return true;
                }
            };
        }

        public void SetupDoneStyleTextField(UITextField textField, bool hasMore = false)
        {
            textField.ReturnKeyType = hasMore ? UIReturnKeyType.Next : UIReturnKeyType.Done;
            textField.ShouldReturn = TextFieldShouldReturn;

        }

        public bool TextFieldShouldReturn(UITextField tf)
        {
            var nextTag = tf.Tag + 1;

            var nextResponder = View.ViewWithTag(nextTag);
            if (nextResponder != null)
            {
                nextResponder.BecomeFirstResponder();
            }
            else
            {
                // No next responder - we're done
                tf.ResignFirstResponder();
            }

            return false; // No line breaks
        }
    }
}