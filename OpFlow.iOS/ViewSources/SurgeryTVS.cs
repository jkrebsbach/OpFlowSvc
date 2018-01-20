using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class SurgeryTVS : UITableViewSource
    {
        private List<Surgery> _surgeries;
        private Dictionary<int, Patient> _surgeryPatients;

        public SurgeryTVS(List<Surgery> surgeries, Dictionary<int, Patient> surgeryPatients)
        {
            _surgeries = surgeries;
            _surgeryPatients = surgeryPatients;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("ScheduleCell", indexPath) as ScheduleCell;

            var surgery = _surgeries[indexPath.Row];
            var patient = _surgeryPatients[surgery.SurgeryID];

            cell?.UpdateCell(surgery, patient);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _surgeries.Count;
        }
    }
}