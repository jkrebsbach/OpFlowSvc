using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class SearchCaseTVS : UITableViewSource
    {
        private readonly List<SurgerySearchResult> _surgeries;

        public SearchCaseTVS(List<SurgerySearchResult> surgeries)
        {
            _surgeries = surgeries;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var surgery = _surgeries[indexPath.Row];

            var cell = 
                tableView.DequeueReusableCell("CaseSearchResultCell", indexPath) as CaseSearchResultCell;

            cell?.UpdateCell(surgery);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _surgeries.Count;
        }
    }
}