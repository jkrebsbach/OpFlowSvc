using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class ScheduleTableCell : UITableViewCell
    {
        public ScheduleTableCell(IntPtr handle) : base(handle)
        { }

        internal abstract void UpdateCell(Surgery surgery, Patient patient);
    }
}