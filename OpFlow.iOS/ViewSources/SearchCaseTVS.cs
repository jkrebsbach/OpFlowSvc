using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class SearchCaseTVS : UITableViewSource
    {
        private readonly List<SurgerySearchModel> _surgeries;

        public SearchCaseTVS(List<SurgerySearchResult> surgeries)
        {
            _surgeries = new List<SurgerySearchModel>();

            surgeries.ForEach(s => _surgeries.Add(new SurgerySearchModel(s)));
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

        public void SelectAll(bool selectAll)
        {
            for (var index = 0; index < _surgeries.Count; index++)
                _surgeries[index].Selected = selectAll;
        }

        public class SurgerySearchModel
        {
            public bool Selected;
            public SurgerySearchResult Surgery;

            public SurgerySearchModel(SurgerySearchResult surgery)
            {
                Surgery = surgery;
            }
        }
    }
}