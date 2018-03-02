using Foundation;
using OpFlow.Data;
using System;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;
using OpFlow.iOS.ViewSources;
using System.Collections.Generic;

namespace OpFlow.iOS
{
    public partial class ScheduleCell : ScheduleTableCell
    {
        public ScheduleCell (IntPtr handle) : base (handle)
        {
        }

        internal override void UpdateCell(SurgerySchedule surgery, Patient patient, SurgeryTVS surgeryTVS)
        {
            CustomFormatting();

            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = string.Format("{0} {1}", patient?.BirthDate.CalculateAge(), patient?.Gender);

            lblPatientName.Text = string.Format("{0}", patient?.Initials ?? "UNK");
            lblProcedure.Text = surgery.ProcedureDescription;
            lblSurgeryTeam.Text = CalculateSurgeryTeam(surgery.SurgeryUsers);
            lblStartTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");

            lblPreferenceCard.Text = surgery.CardDescription ?? "Unassigned";
            lblDuration.Text = "Duration: UNK";
        }

        private string CalculateSurgeryTeam(List<User> surgeryUsers)
        {
            return "DD, ZZ, AA";
        }
    }
}