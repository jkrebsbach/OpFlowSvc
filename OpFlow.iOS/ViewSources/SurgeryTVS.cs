using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class SurgeryTVS : UITableViewSource
    {
        private readonly List<SurgerySearchResult> _surgeries;
        private readonly Dictionary<int, Patient> _surgeryPatients;

        public event EventHandler<SurgerySearchResult> SurgerySelectionEvent;
        public event EventHandler<SurgerySearchResult> DebriefSelectionEvent;

        public event EventHandler<SurgerySearchResult> ReviewSurgeryEvent;

        public SurgeryTVS(List<SurgerySearchResult> surgeries, Dictionary<int, Patient> surgeryPatients)
        {
            _surgeries = surgeries;
            _surgeryPatients = surgeryPatients;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var surgery = _surgeries[indexPath.Row];
            var patient = _surgeryPatients[surgery.SurgeryID];

            ScheduleTableCell cell = null;
            if (surgery.SurgeryStatus == "O")
                cell = tableView.DequeueReusableCell("OpenCaseCell", indexPath) as ScheduleTableCell;
            else
                cell = tableView.DequeueReusableCell("ScheduleCell", indexPath) as ScheduleTableCell;

            cell.ReviewSurgery += ReviewSurgeryEventFired;
            cell?.UpdateCell(surgery, patient, this);

            return cell;
        }

        public void ReviewSurgeryEventFired(object sender, int surgeryId)
        {
            if (ReviewSurgeryEvent != null){
                var surgery = _surgeries.FirstOrDefault(s => s.SurgeryID == surgeryId);
                this.ReviewSurgeryEvent(this, surgery);
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _surgeries.Count;
        }

        public void NavigationButtonEvent(SurgerySearchResult surgery)
        {
            DebriefSelectionEvent?.Invoke(this, surgery);
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var surgery = _surgeries[indexPath.Row];

            SurgerySelectionEvent?.Invoke(this, surgery);
        }
    }
}