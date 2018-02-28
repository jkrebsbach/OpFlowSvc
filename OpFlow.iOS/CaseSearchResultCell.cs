using Foundation;
using System;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CaseSearchResultCell : UITableViewCell
    {
        public CaseSearchResultCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(SurgerySearchResult surgery)
        {
            lblCaseProcedure.Text = $"{surgery.ProcedureDescription} {surgery.ScheduleDate:M/d} {surgery.ScheduleTime:hh\\:mm}";
            lblCaseOverview.Text = $"Room:{surgery.RoomDescription} Team: MK, DR, BW";
        }
    }
}