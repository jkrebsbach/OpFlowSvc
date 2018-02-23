using Foundation;
using System;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SelectUserCell : UITableViewCell
    {
        private UserSelectionTVS.UserSelectionModel _model;

        public SelectUserCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(UserSelectionTVS.UserSelectionModel model)
        {
            _model = model;
            lblUserName.Text = model.User.LastName + ", " + model.User.FirstName;
        }
    }
}