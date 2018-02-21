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
        private readonly List<UserSelectionModel> _users;

        public UserSelectionTVS(List<SurgeryUser> users)
        {
            _users = new List<UserSelectionModel>();

            users.ForEach(u => _users.Add(new UserSelectionModel(u)));
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var user = _users[indexPath.Row];
            
            SelectUserCell cell = null;
            cell = tableView.DequeueReusableCell("SelectUserCell", indexPath) as SelectUserCell;

            cell?.UpdateCell(user);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _users.Count;
        }

        public List<int> SelectedUsers()
        {
            var result = new List<int>();

            _users.Where(u => u.Selected).ToList().ForEach(u => result.Add(u.User.UserID));

            return result;
        }

        public class UserSelectionModel
        {
            public SurgeryUser User { get; }
            public bool Selected { get; set; }

            public UserSelectionModel(SurgeryUser user)
            {
                User = user;
            }
        }
    }
}