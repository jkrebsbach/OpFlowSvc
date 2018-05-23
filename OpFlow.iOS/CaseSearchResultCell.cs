using Foundation;
using System;
using OpFlow.Data;
using UIKit;
using OpFlow.iOS.ViewSources;

namespace OpFlow.iOS
{
    public partial class CaseSearchResultCell : UITableViewCell
    {
        public CaseSearchResultCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(SearchCaseTVS.SurgerySearchModel model)
        {
            var surgery = model.Surgery;
            
            lblCaseProcedure.Text = $"{surgery.ProcedureDescription} {surgery.ScheduleTime:M/d} {surgery.ScheduleTime:hh\\:mm}";
            lblCaseOverview.Text = $"Room:{surgery.RoomDescription} {surgery.SurgeryTeam}";

        }
    }
}