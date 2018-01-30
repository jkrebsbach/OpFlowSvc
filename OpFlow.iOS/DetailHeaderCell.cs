using Foundation;
using System;
using CoreGraphics;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DetailHeaderCell : DetailListCell
    {
        public DetailHeaderCell (IntPtr handle) : base (handle)
        {
        }

        public override void UpdateCell(CaseDetailToken token)
        {
            lblCategory.Text = token.CategoryTitle;
        }

        public void AssignToggleImage(bool hiddenDetails)
        {
            ivToggleArrow.Transform = !hiddenDetails ?
                CGAffineTransform.MakeRotation((float)(Math.PI / 2)) :
                CGAffineTransform.MakeIdentity();
        }
    }
}