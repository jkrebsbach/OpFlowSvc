using Foundation;
using System;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS
{
    public partial class RoomSearchResultCell : UITableViewCell
    {
        public RoomSearchResultCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(RoomSetup roomSetup)
        {
            lblDescription.Text = roomSetup.SetupName;
            lblPosition.Text = roomSetup.PatientPosition;
            lblSurgeonPosition.Text = roomSetup.SurgeonPosition;
        }
    }
}