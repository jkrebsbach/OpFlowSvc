using Foundation;
using OpFlow.Data;
using System;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;
using OpFlow.iOS.ViewSources;
using System.Collections.Generic;
using System.Linq;

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
            lblPatientInfo.Text = $"{patient?.BirthDate.CalculateAge()} {patient?.Gender}";

            var patientName = $"{patient?.LastName} {patient?.FirstName}".Trim();
            if (patientName == string.Empty)
                patientName = patient?.Initials ?? string.Empty;
            if (patientName == string.Empty)
                patientName = "UNK";

            lblPatientName.Text = patientName;
            lblProcedure.Text = surgery.ProcedureDescription;
            lblSurgeryTeam.Text = CalculateSurgeryTeam(surgery.SurgeryUsers);
            lblStartTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");

            lblPreferenceCard.Text = surgery.CardDescription ?? "Unassigned";
            lblDuration.Text = "Duration: " + (surgery.TotalMinutes?.ToString() ?? "UNK");
        }

        private string CalculateSurgeryTeam(List<SurgeryUser> surgeryUsers)
        {
            var result = string.Join(", ", surgeryUsers.Select(s => s.LastName));

            return result;
        }
    }
}