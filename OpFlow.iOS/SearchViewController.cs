using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
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

            txtCase.ValueChanged += SearchCases;

            await LoadDropdowns();
        }

        public async void SearchCases(object sender, EventArgs e)
        {
            int? surgeonId = null;
            int? roomId = null;
            string caseNbr = null;
            DateTime? surgeryDate = null;

            if (txtCase.Text != "")
                caseNbr = txtCase.Text;

            if (txtSurgeon.Text != "")
                surgeonId = _surgeonPicker.GetCurrentId();

            if (txtRoom.Text != "")
                roomId = _roomPicker.GetCurrentId();

            if (txtDate.Text != "")
                surgeryDate = _surgeryDate.Date.ToDateTime().Date;

            if (caseNbr == null && surgeonId == null && roomId == null)
            {
                ShowDialog("Provide filters", "Please provide search criteria");
                return;
            }

            var surgeries = await SurgeryUtil.SearchCases(caseNbr, surgeonId, roomId, surgeryDate);

            var surgeryTableViewSource = new SearchCaseTVS(surgeries);

            CaseSearchTableView.Source = surgeryTableViewSource;
            CaseSearchTableView.ReloadData();
        }

        private async Task LoadDropdowns()
        {
            var rooms = await AppSettings.RoomList(0);
            _roomPicker = new OpFlowTextPicker(txtRoom, rooms.Cast<IBindableEntity>().ToList());

            var specialties = await LookupUtil.GetSpecialties();
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, specialties.Cast<IBindableEntity>().ToList());

            _roomPicker.ValueChanged += SearchCases;
            _surgeonPicker.ValueChanged += SearchCases;
        }
    }
}