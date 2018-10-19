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

            lblCaseProcedure.Text = $"{_surgery.ProcedureDescription} {_surgery.ScheduleTime:M/d} {_surgery.ScheduleTime:hh\\:mm}";
            lblCaseOverview.Text = $"{patient.LastName}, {patient.FirstName} {patient.MiddleInitial} Room:{_surgery.RoomDescription} {_surgery.SurgeryTeam}";

        }
    }
}