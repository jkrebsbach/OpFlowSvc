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
            lblSummary.Text = $"{roomSetup.SetupName}";
            lblOverview.Text = $"{roomSetup.PatientPosition} - {roomSetup.RoomTypeDescription} - {roomSetup.PatientAccess}";
            lblInstruments.Text = roomSetup.EquipmentList();
        }
    }
}