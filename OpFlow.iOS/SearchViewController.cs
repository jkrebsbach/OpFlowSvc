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

        private List<int> _surgeryIds;

        public SearchViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            btnAssign.Hidden = true;
            _beginDate = new OpFlowTextDatePicker(txtBeginDate)
            {
                Mode = UIDatePickerMode.Date
            };
            _endDate = new OpFlowTextDatePicker(txtEndDate)
            {
                Mode = UIDatePickerMode.Date
            };

            _beginDate.DateChanged += ParametersChanged;
            _endDate.DateChanged += ParametersChanged;

            await ExecuteAsyncWebRequest(LoadDropdowns);
        }

        public async void ParametersChanged(object sender, EventArgs e)
        {
            await ExecuteAsyncWebRequest(SearchCases);
        }

        private async Task SearchCases()
        {
            int? surgeonId = null;
            int? roomId = null;
            DateTime? beginDate = null;
            DateTime? endDate = null;

            if (txtSurgeon.Text != "")
                surgeonId = _surgeonPicker.GetCurrentId();

            if (txtRoom.Text != "")
                roomId = _roomPicker.GetCurrentId();

            if (txtBeginDate.Text != "")
                beginDate = _beginDate.Date.ToDateTime().Date;

            if (txtEndDate.Text != "")
                endDate = _endDate.Date.ToDateTime().Date;

            var surgeries = new List<SurgerySearchResult>();
            var patients = new List<Patient>();
            if (surgeonId != null || roomId != null || beginDate != null || endDate != null)
            {
                surgeries = await SurgeryUtil.SearchCases(surgeonId, roomId, beginDate, endDate);

                var patientIds = surgeries.Select(s => s.PatientID).ToList();
                patients = await PatientUtil.GetPatients(patientIds);
            }

            var surgeryTableViewSource = new SearchCaseTVS(surgeries, patients);
            _surgeryIds = surgeries.Select(s => s.SurgeryID).ToList();

            CaseSearchTableView.Source = surgeryTableViewSource;
            CaseSearchTableView.ReloadData();

            btnAssign.Hidden = false;

            surgeryTableViewSource.EntitySelectionEvent += SelectCase;
        }

        async partial void btnAssign_Click(UIButton sender)
        {
            await ExecuteAsyncWebRequest(AssignSurgeries, "Assigning...");

            ShowDialog("Done", "Assignment complete");
        }

        private async Task AssignSurgeries()
        {
            foreach (var surgeryId in _surgeryIds)
                await SurgeryUtil.AssignSurgery(surgeryId);


        }


        protected void SelectCase(Object sender, SurgerySearchResult surgery)
        {
            AppSettings.LoadSurgery(surgery.SurgeryID);
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

            _roomPicker.ValueChanged += ParametersChanged;
            _specialtyPicker.ValueChanged += SpecialtyChanged;
        }

        private async void SpecialtyChanged(object sender, EventArgs e)
        {
            await ExecuteAsyncWebRequest(LoadSurgeons);
        }

        private async Task LoadSurgeons()
        { 
            var specialtyId = _specialtyPicker.GetCurrentId();

            var surgeons = new List<Surgeon>();

            if (specialtyId > 0)
                surgeons = await UserUtil.GetSurgeons(specialtyId ?? 0);
            
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, surgeons.Cast<IBindableEntity>().ToList());

            _surgeonPicker.ValueChanged += ParametersChanged;
        }
    }
}