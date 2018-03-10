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

        public RoomSetupViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            await LoadDropdowns();
            _roomSetups = await LookupUtil.GetRoomSetups();

            UpdateRoomSetups();
        }

        private async Task LoadDropdowns()
        {
            var roomTypes = await LookupUtil.GetRoomTypes();
            roomTypes.Insert(0, new RoomType()
            {
                RoomTypeID = 0,
                RoomTypeDescription = "[Blank]"
            });
            _roomTypePicker = new OpFlowTextPicker(txtRoomType, roomTypes.Cast<IBindableEntity>().ToList());

            var positions = await LookupUtil.GetPatientPositions();
            positions.Insert(0, new PatientPosition()
            {
                PatientPositionID = 0,
                PositionDescription = "[Blank]"
            });
            _positionPicker = new OpFlowTextPicker(txtPosition, positions.Cast<IBindableEntity>().ToList());

            _roomTypePicker.ValueChanged += FiltersChanged;
            _positionPicker.ValueChanged += FiltersChanged;
        }

        private void FiltersChanged(object sender, EventArgs e)
        {
            UpdateRoomSetups();
        }

        private void UpdateRoomSetups()
        {
            var roomTypeId = _roomTypePicker.GetCurrentId();
            var position = _positionPicker.GetCurrentId();

            // If a patient position is selected, apply filter
            var roomSetups = _roomSetups.Where(rs => ((roomTypeId ?? 0) == 0 || rs.RoomTypeID == roomTypeId) && 
                                               ((position ?? 0) == 0 || rs.PatientPosition.ToUpper() == txtPosition.Text.ToUpper())).ToList();
            var roomTableViewSource = new RoomSetupSearchTVS(roomSetups);

            RoomSearchTableView.Source = roomTableViewSource;
            RoomSearchTableView.ReloadData();

            var roomSetup = roomSetups.FirstOrDefault(rs => rs.RoomSetupID == AppSettings.CurrentRoomSetup);
            if (roomSetup != null)
            {
                var selectionIndex = roomSetups.IndexOf(roomSetup);
                RoomSearchTableView.SelectRow(NSIndexPath.FromRowSection(selectionIndex, 0), true, UITableViewScrollPosition.Bottom);
            }

            roomTableViewSource.EntitySelectionEvent += SelectRoomSetup;
        }

        protected async void SelectRoomSetup(Object sender, IBindableEntity roomSetup)
        {
            await SurgeryUtil.AssignRoomSetup(AppSettings.CurrentSurgery ?? -1, roomSetup.GetID());
            AppSettings.CurrentRoomSetup = roomSetup.GetID();

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
        }
    }
}