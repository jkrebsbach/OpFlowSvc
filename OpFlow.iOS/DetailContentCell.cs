using Foundation;
using System;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DetailContentCell : DetailListCell
    {
        public DetailContentCell (IntPtr handle) : base (handle)
        {
        }

        public override void UpdateCell(CaseDetailToken token)
        {
            txtDetail.Text = token.DetailText;

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