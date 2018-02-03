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
        private readonly bool _allowEdit;

        public event EventHandler<List<CaseDetailToken>> DetailListConfirmedEvent;

        public DetailListTVS(List<CaseDetailToken> details, bool allowEdit)
        {
            _detailTokens = details;
            _allowEdit = allowEdit;

            _hiddenDetails = new List<bool>();

            _detailTokens.ForEach(dt => _hiddenDetails.Add(false));
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var tokenIndex = (int)(indexPath.Row / 2);
            var header = (indexPath.Row % 2 == 0);

            var lastRow = (indexPath.Row == _detailTokens.Count * 2);

            if (lastRow)
                return tableView.DequeueReusableCell("DetailConfirmCell", indexPath);

            var token = _detailTokens[tokenIndex];
            var hiddenDetails = _hiddenDetails[tokenIndex];

            DetailListCell cell;
            if (header)
            {
                var headerCell = tableView.DequeueReusableCell("DetailHeaderCell", indexPath) as DetailHeaderCell;
                headerCell?.AssignToggleImage(hiddenDetails);

                cell = headerCell;
            }
            else
            {
                var detailCell = tableView.DequeueReusableCell("DetailContentCell", indexPath) as DetailContentCell;
                detailCell?.AllowEdit(_allowEdit);

                cell = detailCell;
                if (cell != null)
                    cell.Hidden = hiddenDetails;
            }

            cell?.UpdateCell(token);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var cellCount = _detailTokens.Count * 2;

            if (_allowEdit)
                cellCount++; // Include confirm cell

            return cellCount;
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