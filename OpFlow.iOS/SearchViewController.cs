using Foundation;
using System;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;
using System.Collections.Generic;

namespace OpFlow.iOS
{
    public partial class SearchViewController : OpFlowViewController
    {
        private OpFlowTextPicker _roomPicker;
        private OpFlowTextPicker _specialtyPicker;
        private OpFlowTextPicker _surgeonPicker;

        private OpFlowTextDatePicker _beginDate;
        private OpFlowTextDatePicker _endDate;

        public SearchViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupDoneStyleTextField(txtCase, false);
            SetupDoneStyleTextField(txtPatient, false);

            _beginDate = new OpFlowTextDatePicker(txtBeginDate)
            {
                Mode = UIDatePickerMode.Date
            };
            _endDate = new OpFlowTextDatePicker(txtEndDate)
            {
                Mode = UIDatePickerMode.Date
            };

            txtCase.AddTarget((sender, e) =>
            {
                SearchCases(sender, e);
            }, UIControlEvent.EditingDidEnd);

            await LoadDropdowns();
        }

        public async void SearchCases(object sender, EventArgs e)
        {
            int? surgeonId = null;
            int? roomId = null;
            string caseNbr = null;
            DateTime? beginDate = null;
            DateTime? endDate = null;

            if (txtCase.Text != "")
                caseNbr = txtCase.Text;

            if (txtSurgeon.Text != "")
                surgeonId = _surgeonPicker.GetCurrentId();

            if (txtRoom.Text != "")
                roomId = _roomPicker.GetCurrentId();

            if (txtBeginDate.Text != "")
                beginDate = _beginDate.Date.ToDateTime().Date;
            
            if (txtEndDate.Text != "")
                endDate = _endDate.Date.ToDateTime().Date;

            var surgeries = new List<SurgerySearchResult>();
            if (caseNbr != null || surgeonId != null || roomId != null)
            {
                surgeries = await SurgeryUtil.SearchCases(caseNbr, surgeonId, roomId, beginDate, endDate);
            }

            var surgeryTableViewSource = new SearchCaseTVS(surgeries);

            CaseSearchTableView.Source = surgeryTableViewSource;
            CaseSearchTableView.ReloadData();

            surgeryTableViewSource.EntitySelectionEvent += SelectCase;
        }

        protected void SelectCase(Object sender, SurgerySearchResult surgery)
        {
            AppSettings.LoadSurgery(surgery.SurgeryID, surgery.PatientID);

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
            
        }

        private async Task LoadDropdowns()
        {
            var rooms = await AppSettings.RoomList(0);

            rooms.Insert(0, new Room()
            {
                RoomID = 0,
                RoomDescription = "[Blank]"
            });
            _roomPicker = new OpFlowTextPicker(txtRoom, rooms.Cast<IBindableEntity>().ToList());

            var specialties = await LookupUtil.GetSpecialties();
            specialties.Insert(0, new Specialty()
            {
                SpecialtyID = 0,
                SpecialtyDescription = "[Blank]"
            });
            _specialtyPicker = new OpFlowTextPicker(txtSpecialty, specialties.Cast<IBindableEntity>().ToList());

            _roomPicker.ValueChanged += SearchCases;
            _specialtyPicker.ValueChanged += SpecialtySelected;
        }

        private async void SpecialtySelected(object sender, EventArgs e)
        {
            var specialtyId = _specialtyPicker.GetCurrentId();

            var surgeons = new List<Surgeon>();

            if (specialtyId > 0)
                surgeons = await UserUtil.GetSurgeons(specialtyId);
            
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, surgeons.Cast<IBindableEntity>().ToList());

            _surgeonPicker.ValueChanged += SearchCases;
        }
    }
}