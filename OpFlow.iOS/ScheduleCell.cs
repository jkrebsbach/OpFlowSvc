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
        int _surgeryId;

        public ScheduleCell (IntPtr handle) : base (handle)
        {
        }

        partial void btnReview_Click(UIButton sender)
        {
            if (this.ReviewSurgery != null){
                this.ReviewSurgery(this, _surgeryId);
            }
        }

        internal override void UpdateCell(SurgerySearchResult surgery, Patient patient, SurgeryTVS surgeryTVS)
        {
            _surgeryId = surgery.SurgeryID;

            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = $"{patient?.BirthDate.CalculateAge()} {patient?.Gender}";

            var patientName = $"{patient?.LastName}, {patient?.FirstName} {patient?.MiddleInitial}".Trim();
            if (patientName == string.Empty)
                patientName = patient?.LastName ?? string.Empty;
            if (patientName == string.Empty)
                patientName = "UNK";

            var surgeryUser = surgery
                .SurgeryUsers.FirstOrDefault(s =>
                      s.RoleID == 1 && s.UserID == AppSettings.CurrentUser.UserID);

            btnReview.Hidden = (surgeryUser == null || surgeryUser.SurgeryReview != null);

            lblPatientName.Text = patientName;

            var procedureDescription = surgery.ProcedureDescription ?? "";
            if (procedureDescription.Length > 75)
            {
                procedureDescription = procedureDescription.Substring(0, 75) + "...";
            }
            lblProcedure.Text = procedureDescription;
            lblSurgeryTeam.Text = CalculateSurgeryTeam(surgery.SurgeryUsers);
            lblStartTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");

            lblDuration.Text = "Duration: " + (surgery.TotalMinutes?.ToString() ?? "UNK");

            var step = "Scheduled";
            if (surgery.SurgeryStatus == "C")
                step = "Closed";

            lblFlowStep.Text = $"Start: {surgery.ActualStartTime?.ToString("HH:mm") ?? "Scheduled"} Step:{step}";
            lblStaffChange.Text = $"Staff Change: {surgery.StaffChange ?? "ASK"}";
        }

        private string CalculateSurgeryTeam(List<SurgeryUser> surgeryUsers)
        {
            if (!surgeryUsers.Any())
                return "NONE";

            var result = string.Join(", ", surgeryUsers.Select(s => s.LastName));

            return result;
        }
    }
}