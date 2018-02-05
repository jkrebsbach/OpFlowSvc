using System;
using System.Collections.Generic;
using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class DashboardTVS: UITableViewSource
    {
        private List<FlowTiming> _flowTimings;

        public DashboardTVS(List<FlowTiming> flowTimings)
        {
            _flowTimings = flowTimings;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var timing = _flowTimings[indexPath.Row];
            
            var cell = tableView.DequeueReusableCell("DashboardStepCell", indexPath) as DashboardStepCell;
            
            cell?.UpdateCell(timing);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _flowTimings.Count;
        }
    }
}
