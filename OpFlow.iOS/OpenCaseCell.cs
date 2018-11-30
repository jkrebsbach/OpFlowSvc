using System;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;

using Foundation;
using UIKit;
using OpFlow.iOS.ViewSources;
using System.Collections.Generic;
using System.Linq;

namespace OpFlow.iOS
{
    public partial class OpenCaseCell :  ScheduleTableCell
    {
        private SurgeryTVS _surgeryTvs;
        private SurgerySearchResult _surgery;

        public OpenCaseCell (IntPtr handle) : base (handle)
        {
        }

        partial void btnDebrief_Click(UIButton sender)
        {
            _surgeryTvs?.NavigationButtonEvent(_surgery);
        }

        internal override void UpdateCell(SurgerySearchResult surgery, Patient patient, SurgeryTVS surgeryTVS)
        {
            _surgeryTvs = surgeryTVS;
            _surgery = surgery;

            var patientName = $"{patient?.LastName}, {patient?.FirstName} {patient?.MiddleInitial}".Trim();
            if (patientName == string.Empty)
                patientName = patient?.LastName ?? string.Empty;
            if (patientName == string.Empty)
                patientName = "UNK";

            lblLocation.Text = surgery.RoomDescription;
            lblPatientInfo.Text = string.Format("{0} {1}", patient?.BirthDate.CalculateAge(), patient?.Gender);

            lblPatientName.Text = patientName;

            var procedureDescription = _surgery.ProcedureDescription ?? "";
            if (procedureDescription.Length > 75)
            {
                procedureDescription = procedureDescription.Substring(0, 75) + "...";
            }
            lblProcedure.Text = procedureDescription;
            lblSurgeryTeam.Text = CalculateSurgeryTeam(surgery.SurgeryUsers);
            lblSurgeryTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");

            lblFlowStep.Text = $"Start: {surgery.ActualStartTime?.ToString("HH:mm")} Step:{surgery.FlowStepDescription}";
            lblStaffChange.Text = $"Staff Change: {surgery.StaffChange ?? "YES"}";
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