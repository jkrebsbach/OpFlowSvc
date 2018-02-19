using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public static class OpFlowHelperMethod
    {
        public static void SetupDoneStyleTextBox(this UITextView textView)
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

        public static void SetupDoneStyleTextField(this UITextField textField)
        {
            textField.ReturnKeyType = UIReturnKeyType.Done;
            textField.ShouldReturn = TextFieldShouldReturn;

        }

        public static bool TextFieldShouldReturn(UITextField tf)
        {
            tf.ResignFirstResponder();
            return true;
        }
    }
}