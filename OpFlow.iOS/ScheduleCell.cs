using Foundation;
using OpFlow.Data;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ScheduleCell : UITableViewCell
    {
        public ScheduleCell (IntPtr handle) : base (handle)
        {
        }

        internal void UpdateCell(Surgery surgery)
        {
            lblLocation.Text = surgery.BundleDescription;
            lblPatientInfo.Text = surgery.BundleDescription;
            lblPatientName.Text = surgery.CardDescription;
            lblProcedure.Text = surgery.BundleDescription;
        }
    }
}