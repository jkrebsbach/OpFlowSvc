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
            lblUserName.Text = user.LastName + ", " + user.FirstName;
        }
    }
}