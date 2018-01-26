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
        private readonly List<Surgery> _surgeries;
        private readonly Dictionary<int, Patient> _surgeryPatients;

        public event EventHandler<Surgery> SurgerySelectionEvent;

        public SurgeryTVS(List<Surgery> surgeries, Dictionary<int, Patient> surgeryPatients)
        {
            _surgeries = surgeries;
            _surgeryPatients = surgeryPatients;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var surgery = _surgeries[indexPath.Row];
            var patient = _surgeryPatients[surgery.SurgeryID];

            var cell = tableView.DequeueReusableCell("ScheduleCell", indexPath) as ScheduleCell;

            cell?.UpdateCell(surgery, patient);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _surgeries.Count;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var surgery = _surgeries[indexPath.Row];

            SurgerySelectionEvent?.Invoke(this, surgery);
        }
    }
}