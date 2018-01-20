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

        public SurgeryTVS(List<Surgery> surgeries)
        {
            _surgeries = surgeries;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("ScheduleCell", indexPath) as ScheduleCell;

            var surgery = _surgeries[indexPath.Row];

            cell?.UpdateCell(surgery);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _surgeries.Count;
        }
    }
}