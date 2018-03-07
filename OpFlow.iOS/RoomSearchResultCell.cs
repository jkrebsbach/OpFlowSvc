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
            lblDetails.Text = $"{roomSetup.SetupName} - {roomSetup.PatientPosition}";
            lblSummary.Text = roomSetup.SurgeonPosition;
        }
    }
}