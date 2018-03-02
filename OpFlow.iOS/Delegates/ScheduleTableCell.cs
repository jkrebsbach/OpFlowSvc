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
        }

        internal abstract void UpdateCell(SurgerySchedule surgery, Patient patient, SurgeryTVS surgeryTVS);
    }
}