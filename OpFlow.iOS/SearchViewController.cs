using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SearchViewController : OpFlowViewController
    {
        private OpFlowTextPicker _roomPicker;
        private OpFlowTextPicker _surgeonPicker;

        private OpFlowTextDatePicker _surgeryDate;

        public SearchViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupDoneStyleTextField(txtCase, true);
            SetupDoneStyleTextField(txtPatient, true);

            _surgeryDate = new OpFlowTextDatePicker(txtDate)
            {
                Mode = UIDatePickerMode.Date
            };

            await LoadDropdowns();
        }

        private async Task LoadDropdowns()
        {
            var rooms = await AppSettings.RoomList(0);
            _roomPicker = new OpFlowTextPicker(txtRoom, rooms.Cast<IBindableEntity>().ToList());

            var specialties = await LookupUtil.GetSpecialties();
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, specialties.Cast<IBindableEntity>().ToList());
        }
    }
}