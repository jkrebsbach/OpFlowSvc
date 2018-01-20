using Foundation;
using OpFlow.Data;
using System;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ScheduleCell : UITableViewCell
    {
        public ScheduleCell (IntPtr handle) : base (handle)
        {
        }

        internal void UpdateCell(Surgery surgery, Patient patient)
        {
            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = string.Format("{0} {1}", patient?.BirthDate.CalculateAge(), patient?.Gender);
            lblPatientName.Text = string.Format("{0}, {1}", patient?.LastName, patient?.FirstName);
            lblProcedure.Text = surgery.ProcedureDescription;
            lblStartTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");
            
        }
    }
}