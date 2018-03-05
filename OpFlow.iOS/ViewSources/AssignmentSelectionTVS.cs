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
    public class AssignmentSelectionTVS : UITableViewSource
    {
        private readonly List<IBindableEntity> _entities;

        public event EventHandler<IBindableEntity> EntitySelectionEvent;

        public AssignmentSelectionTVS(List<IBindableEntity> entities)
        {
            _entities = entities;
        }
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var entity = _entities[indexPath.Row];

            var reuseIdentifier = entity is Card ? "SelectCardCell" : "SelectFlowCell";

            var cell = tableView.DequeueReusableCell(reuseIdentifier, indexPath) as BindableTableViewCell;
            cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;

            cell?.UpdateCell(entity);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _entities.Count;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var entity = _entities[indexPath.Row];

            EntitySelectionEvent?.Invoke(this, entity);
        }
    }
}