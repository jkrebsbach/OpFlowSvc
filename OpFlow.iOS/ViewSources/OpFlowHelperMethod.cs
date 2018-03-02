using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public static class OpFlowHelperMethod
    {
        public static void SetupDoneStyleTextBox(this UITextView textView)
        {
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

        public static UIColor RoleBackgroundColorMapping(this RoleEnum roleId)
        {
            switch (roleId)
            {
                case RoleEnum.Surgeon:
                    return UIColor.Green;
                case RoleEnum.Circulator:
                    return UIColor.Blue;
                case RoleEnum.ScrubTech:
                    return UIColor.Black;
                case RoleEnum.Anesthesiologist:
                    return UIColor.Orange;
                case RoleEnum.Schedule:
                    return UIColor.Red;
                case RoleEnum.Representative:
                    return UIColor.Red;
                case RoleEnum.Administration:
                    return UIColor.Purple;
                default:
                    return UIColor.White;
            }
        }

        public static UIColor RoleTextColorMapping(this RoleEnum roleId)
        {
            switch (roleId)
            {
                case RoleEnum.ScrubTech:
                    return UIColor.White;
                default:
                    return UIColor.Black;
            }
        }
    }
}