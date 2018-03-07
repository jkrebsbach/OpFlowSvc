using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class RoomSetupViewController : OpFlowViewController
    {
        private List<RoomSetup> _roomSetups;

        private OpFlowTextPicker _roomTypePicker;
        private OpFlowTextPicker _positionPicker;

        private int _roomTypeId;

        public RoomSetupViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            await LoadDropdowns();
            _roomSetups = await LookupUtil.GetRoomSetups();
        }

        private async Task LoadDropdowns()
        {
            var roomTypes = await LookupUtil.GetRoomTypes();
            _roomTypePicker = new OpFlowTextPicker(txtRoomType, roomTypes.Cast<IBindableEntity>().ToList());

            _roomTypePicker.ValueChanged += UpdateRoomType;
        }

        private async void UpdateRoomType(object sender, EventArgs e)
        {
            var roomTypeId = _roomTypePicker.GetCurrentId();

            if (roomTypeId <= 0 || _roomTypeId == roomTypeId) return;

            // Reset room whenever the room type changes

            _roomTypeId = roomTypeId;

            var rooms = await LookupUtil.GetRooms();
            rooms = rooms.Where(r => r.RoomTypeID == roomTypeId).ToList();

            _positionPicker = new OpFlowTextPicker(txtPosition, rooms.Cast<IBindableEntity>().ToList());

            var roomSetups = _roomSetups.Where(rs => rs.RoomTypeID == roomTypeId).ToList();
            var roomTableViewSource = new RoomSetupSearchTVS(roomSetups);

            RoomSearchTableView.Source = roomTableViewSource;
            RoomSearchTableView.ReloadData();
        }
    }
}