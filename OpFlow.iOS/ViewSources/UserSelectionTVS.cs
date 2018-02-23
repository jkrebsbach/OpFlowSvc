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
    public class UserSelectionTVS : UITableViewSource
    {
        private readonly List<User> _users;

        public event EventHandler<User> UserSelectionEvent;

        public UserSelectionTVS(List<User> users)
        {
            _users = users;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var user = _users[indexPath.Row];
            
            var cell = tableView.DequeueReusableCell("SelectUserCell", indexPath) as SelectUserCell;

            cell?.UpdateCell(user);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _users.Count;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var user = _users[indexPath.Row];

            UserSelectionEvent?.Invoke(this, user);
        }
    }
}