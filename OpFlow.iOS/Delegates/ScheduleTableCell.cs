using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class ScheduleTableCell : UITableViewCell
    {
        public ScheduleTableCell(IntPtr handle) : base(handle)
        { }

        protected void CustomFormatting()
        {
            var bottomBorder = new CALayer
            {
                Frame = new CGRect(0f, this.Frame.Height - 1, this.Frame.Width, 1.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            Layer.AddSublayer(bottomBorder);

            var topBorder = new CALayer
            {
                Frame = new CGRect(0f, 0f, this.Frame.Width, 1.0f),
                BackgroundColor = UIColor.Black.CGColor
            };
            Layer.AddSublayer(topBorder);
        }

        internal abstract void UpdateCell(Surgery surgery, Patient patient, SurgeryTVS surgeryTVS);
    }
}