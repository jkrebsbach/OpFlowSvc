using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.AndroidApp.Adapters;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public abstract class CaseDetailFragment : OpFlowFragmentBase
    {
        private TextView _txtProcedureStart;
        private TextView _txtLocation;
        private TextView _txtPatientName;
        private TextView _txtPatientAge;
        private TextView _txtPatientBMI;
        private TextView _txtPatientSex;
        private TextView _txtProcedure;

        private ExpandableListView _lvCaseDetails;

        private Patient _patient;
        private Surgery _surgery;

        protected Patient Patient => _patient;
        protected Surgery Surgery => _surgery;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.CaseDetail;
            base.OnCreateView(inflater, container, savedInstanceState);
            
            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.CaseDetail, container, false);

            _lvCaseDetails = rootView.FindViewById<ExpandableListView>(Resource.Id.lvCaseDetails);

            _txtPatientName = rootView.FindViewById<TextView>(Resource.Id.txtPatientName);
            _txtProcedureStart = rootView.FindViewById<TextView>(Resource.Id.txtProcedureStart);
            _txtLocation = rootView.FindViewById<TextView>(Resource.Id.txtLocation);
            _txtPatientAge = rootView.FindViewById<TextView>(Resource.Id.txtPatientAge);
            _txtPatientBMI = rootView.FindViewById<TextView>(Resource.Id.txtPatientBMI);
            _txtPatientSex = rootView.FindViewById<TextView>(Resource.Id.txtPatientSex);
            _txtProcedure = rootView.FindViewById<TextView>(Resource.Id.txtProcedure);

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                if (AppSettings.CurrentSurgery != null)
                {
                    await LoadCase(AppSettings.CurrentSurgery.Value);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected abstract List<CaseDetailToken> GetDetailTokens();

        private async Task LoadCase(int surgeryId)
        {
            try
            {
                _surgery = null;
                _patient = null;

                var locationId = AppSettings.CurrentUser.LocationID;
                var providerId = AppSettings.CurrentUser.ProviderID;

                _surgery = await SurgeryUtil.GetSurgery(surgeryId, providerId, locationId);

                if (_surgery != null)
                {
                    _txtProcedureStart.Text = string.Format(_surgery.ScheduleTime.ToString(@"hh\:mm"));
                    _txtProcedure.Text = _surgery.ProcedureDescription;

                    var room = await AppSettings.GetRoom(_surgery.LocationID, _surgery.RoomID);
                    
                    _txtLocation.Text = room?.RoomDescription ?? "Room not found";

                    _patient = await PatientUtil.GetPatient(_surgery.PatientID);

                    if (_patient != null)
                    {
                        _txtPatientName.Text = string.Format("{0} {1}", _patient.FirstName, _patient.LastName);
                        _txtPatientAge.Text = _patient.BirthDate.CalculateAge().ToString();
                        _txtPatientSex.Text = _patient.Sex.ToString(CultureInfo.InvariantCulture);
                        _txtPatientBMI.Text = _patient.BMI.ToString(CultureInfo.InvariantCulture);
                        
                        _lvCaseDetails.SetAdapter(new CaseDetailListAdapter(Activity, GetDetailTokens()));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}