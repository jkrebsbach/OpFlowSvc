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
            lblRole.Text = user.RoleID.ToString();
            lblUserName.Text = string.Format("{0}, {1}", 
                user.LastName, user.FirstName);
        }
    }
}