using Foundation;
using System;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SelectUserCell : UITableViewCell
    {
        public SelectUserCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(User user)
        {
            lblUserName.Text = string.Format("{0}: {1}, {2}", 
                user.RoleID.ToString(), user.LastName, user.FirstName);
        }
    }
}