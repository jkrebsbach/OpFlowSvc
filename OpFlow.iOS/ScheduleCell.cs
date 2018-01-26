using Foundation;
using OpFlow.Data;
using System;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ScheduleCell : ScheduleTableCell
    {
        public ScheduleCell (IntPtr handle) : base (handle)
        {
        }

        internal override void UpdateCell(Surgery surgery, Patient patient)
        {
            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = string.Format("{0} {1}", patient?.BirthDate.CalculateAge(), patient?.Gender);
            //lblPatientName.Text = string.Format("{0}, {1}", patient?.LastName, patient?.FirstName);
            lblPatientName.Text = string.Format("{0}", patient?.Initials ?? "UNK");
            lblProcedure.Text = surgery.ProcedureDescription;
            lblStartTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");
        }
    }
}