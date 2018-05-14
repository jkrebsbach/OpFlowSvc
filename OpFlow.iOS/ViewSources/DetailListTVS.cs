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
        private readonly List<DetailItem> _detailItems;
        private readonly bool _allowEdit;

        public event EventHandler<List<DetailItem>> DetailListConfirmedEvent;

        public DetailListTVS(List<DetailItem> details, bool allowEdit)
        {
            _detailItems = details;
            _allowEdit = allowEdit;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var lastRow = (indexPath.Row == _detailItems.Count);

            if (lastRow)
                return tableView.DequeueReusableCell("DetailConfirmCell", indexPath);

            var token = _detailItems[indexPath.Row];
            UITableViewCell cell;
            if (token is CaseDetailCategory)
            {
                var hiddenDetails = (token as CaseDetailCategory).HiddenDetails;

                var headerCell = tableView.DequeueReusableCell("DetailHeaderCell", indexPath) as DetailHeaderCell;
                headerCell?.AssignToggleImage(hiddenDetails);

                cell = headerCell;
                headerCell?.UpdateCell(token as CaseDetailCategory);
            }
            else if (token is CaseDetailToken)
            {
                var detailToken = token as CaseDetailToken;
                var hiddenDetails = detailToken.Category.HiddenDetails;
                
                var detailCellType = (_allowEdit ? "DetailEditableContentCell" : "DetailContentCell");
                if (detailToken.DetailType == CaseDetailToken.DetailTypes.ImageDetail)
                    detailCellType = "DetailImageCell";

                var detailCell = tableView.DequeueReusableCell(detailCellType, indexPath) as DetailListCell;

                cell = detailCell;
                if (cell != null)
                    cell.Hidden = hiddenDetails;

                detailCell?.UpdateCell(detailToken);
            }
            else
            {
                throw new Exception("Unsupported Detail Type");
            }

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var cellCount = _detailItems.Count;

            if (_allowEdit)
                cellCount++; // Include confirm cell

            return cellCount;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedCell = tableView.CellAt(indexPath);

            if (selectedCell is DetailConfirmCell confirmCell)
            {
                DetailListConfirmedEvent?.Invoke(this, _detailItems);

            }
            else if (selectedCell is DetailHeaderCell headerCell)
            {
                var category = _detailItems[indexPath.Row] as CaseDetailCategory;

                // Toggle visibility of detail cells
                category.HiddenDetails = !category.HiddenDetails;

                for (var index = 0; index < category.Tokens.Count; index++)
                {
                    var detailIndexPath = NSIndexPath.FromRowSection(indexPath.Row + 1 + index, indexPath.Section);

                    var detailCell = tableView.CellAt(detailIndexPath);

                    headerCell.AssignToggleImage(category.HiddenDetails);

                    if (detailCell == null)
                    {
                        detailCell = tableView.DequeueReusableCell(_detailCellType, indexPath) as DetailListCell;
                    }

                    // Assign visibility to Hidden flag
                    detailCell.Hidden = category.HiddenDetails;
                }

                tableView.BeginUpdates();
                tableView.EndUpdates();
            }
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var defaultSize = 40.0f;

            if (indexPath.Row >= _detailItems.Count)
                return defaultSize;

            var item = _detailItems[indexPath.Row];

            if (item is CaseDetailToken token)
            {
                return token.Category.HiddenDetails ? 0.0f : defaultSize * 2;
            }
            
            return defaultSize;
        }
    }
}