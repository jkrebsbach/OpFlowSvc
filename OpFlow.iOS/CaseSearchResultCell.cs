using Foundation;
using System;
using OpFlow.Data;
using UIKit;
using OpFlow.iOS.ViewSources;
using System.Collections.Generic;
using System.Linq;

namespace OpFlow.iOS
{
    public partial class CaseSearchResultCell : UITableViewCell
    {
        private SearchCaseTVS _searchTvs;
        SurgerySearchResult _surgery;

        public CaseSearchResultCell (IntPtr handle) : base (handle)
        {
        }

        partial void btnAdd_Clicked(UIButton sender)
        {
            _searchTvs.AddCaseButtonEvent(_surgery);
        }

        public void UpdateCell(SearchCaseTVS.SurgerySearchModel model, Patient patient, SearchCaseTVS searchTvs)
        {
            _searchTvs = searchTvs;
            _surgery = model.Surgery;

            var procedureDescription = _surgery.ProcedureDescription ?? "";
            if (procedureDescription.Length > 75)
            {
                procedureDescription = procedureDescription.Substring(0, 75) + "...";
            }

            lblCaseProcedure.Text = $"{_surgery.ScheduleTime:M/d} {_surgery.ScheduleTime:hh\\:mm} {procedureDescription}";
            lblCaseTeam.Text = CalculateSurgeryTeam(_surgery.SurgeryUsers);
            lblPatientName.Text = $"{patient.LastName}, {patient.FirstName} {patient.MiddleInitial}".Trim();
            lblPatientOverview.Text = $"{patient.PatientAge} {patient.Gender} {_surgery.RoomDescription}";

            var step = _surgery.FlowStepDescription ?? "Scheduled";
            if (_surgery.SurgeryStatus == "C")
                step = "Closed";

            lblCaseTiming.Text = $"Start: {_surgery.ActualStartTime?.ToString("HH:mm") ?? "Scheduled"} Step:{step}";
            lblStaffChange.Text = $"Staff Change: {_surgery.StaffChange ?? "ASK"}";
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