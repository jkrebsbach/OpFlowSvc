using Foundation;
using System;
using OpFlow.Data;
using UIKit;
using OpFlow.iOS.ViewSources;

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
            lblCaseOverview.Text = $"{patient.LastName}, {patient.FirstName} {patient.MiddleInitial} Room:{_surgery.RoomDescription} {_surgery.SurgeryTeam}";

        }
    }
}