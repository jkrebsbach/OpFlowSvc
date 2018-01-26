using System;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;

using Foundation;
using UIKit;

namespace OpFlow.iOS
{
    public partial class OpenCaseCell :  ScheduleTableCell
    {
        public OpenCaseCell (IntPtr handle) : base (handle)
        {
        }


        internal override void UpdateCell(Surgery surgery, Patient patient)
        {
            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = string.Format("{0} {1}", patient?.BirthDate.CalculateAge(), patient?.Gender);
            //lblPatientName.Text = string.Format("{0}, {1}", patient?.LastName, patient?.FirstName);
            lblPatientName.Text = string.Format("{0}", patient?.Initials);
            lblProcedure.Text = surgery.ProcedureDescription;
            lblSurgeryTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");
        }
    }
}