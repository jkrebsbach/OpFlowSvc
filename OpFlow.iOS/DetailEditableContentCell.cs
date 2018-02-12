using Foundation;
using System;
using UIKit;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;

namespace OpFlow.iOS
{
    public partial class DetailEditableContentCell : DetailListCell
    {
        public DetailEditableContentCell (IntPtr handle) : base (handle)
        {
        }


        public override void UpdateCell(CaseDetailToken token)
        {
            txtDetail.Text = token.DetailText;

            txtDetail.Layer.BorderWidth = 2.0f;

            txtDetail.ReturnKeyType = UIReturnKeyType.Done;

            txtDetail.ShouldChangeText = (text, range, replacementString) =>
            {
                if (replacementString.Equals("\n"))
                {
                    txtDetail.EndEditing(true);
                    return false;
                }
                else
                {
                    return true;
                }
            };
        }
    }
}