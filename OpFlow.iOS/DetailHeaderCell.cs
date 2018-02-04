using Foundation;
using System;
using CoreGraphics;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;
using CoreAnimation;

namespace OpFlow.iOS
{
    public partial class DetailHeaderCell : DetailListCell
    {
        public DetailHeaderCell (IntPtr handle) : base (handle)
        {
        }

        public override void UpdateCell(CaseDetailToken token)
        {
            var bottomBorder = new CALayer
            {
                Frame = new CGRect(0f, this.Frame.Height, this.Frame.Width, 1.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            Layer.AddSublayer(bottomBorder);

            var topBorder = new CALayer
            {
                Frame = new CGRect(0f, 0f, this.Frame.Width, 1.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            Layer.AddSublayer(topBorder);


            lblCategory.Text = token.CategoryTitle;
        }

        public void AssignToggleImage(bool hiddenDetails)
        {
            ivToggleArrow.Transform = !hiddenDetails ?
                CGAffineTransform.MakeRotation((float)(Math.PI / 2)) :
                CGAffineTransform.MakeRotation((float)Math.PI);
        }
    }
}