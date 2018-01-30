using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class DetailListTVS : UITableViewSource
    {
        private readonly List<CaseDetailToken> _detailTokens;
        private readonly List<bool> _hiddenDetails;

        public event EventHandler<List<CaseDetailToken>> DetailListConfirmedEvent;

        public DetailListTVS(List<CaseDetailToken> details)
        {
            _detailTokens = details;

            _hiddenDetails = new List<bool>();

            _detailTokens.ForEach(dt => _hiddenDetails.Add(true));
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var tokenIndex = (int)(indexPath.Row / 2);
            var header = (indexPath.Row % 2 == 0);

            var lastRow = (indexPath.Row == _detailTokens.Count * 2);

            if (lastRow)
                return tableView.DequeueReusableCell("DetailConfirmCell", indexPath);

            var token = _detailTokens[tokenIndex];

            DetailListCell cell = null;
            if (header)
                cell = tableView.DequeueReusableCell("DetailHeaderCell", indexPath) as DetailListCell;
            else
            {
                cell = tableView.DequeueReusableCell("DetailContentCell", indexPath) as DetailListCell;

                cell.Hidden = _hiddenDetails[tokenIndex];
            }

            cell?.UpdateCell(token);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _detailTokens.Count * 2 + 1;  // Include confirm cell
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var tokenIndex = (int)(indexPath.Row / 2);
            var selectedCell = tableView.CellAt(indexPath);

            if (selectedCell is DetailConfirmCell confirmCell)
            {
                DetailListConfirmedEvent?.Invoke(this, _detailTokens);

            }
            else if (selectedCell is DetailHeaderCell headerCell)
            {
                // Toggle visibility of detail
                var hiddenDetails = !_hiddenDetails[tokenIndex];
                _hiddenDetails[tokenIndex] = hiddenDetails;

                var detailIndexPath = NSIndexPath.FromRowSection(indexPath.Row + 1, indexPath.Section);

                var detailCell = tableView.CellAt(detailIndexPath);

                headerCell.AssignToggleImage(hiddenDetails);

                if (detailCell == null)
                {
                    detailCell = tableView.DequeueReusableCell("DetailContentCell", indexPath) as DetailListCell;
                }

                // Assign visibility to Hidden flag
                detailCell.Hidden = hiddenDetails;

                tableView.BeginUpdates();
                tableView.EndUpdates();
            }
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var tokenIndex = (int)(indexPath.Row / 2);
            var header = (indexPath.Row % 2 == 0);

            if (!header && _hiddenDetails[tokenIndex])
            {
                return 0.0f;
            }

            return 40f;
        }
    }
}