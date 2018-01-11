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
using OpFlow.Android.Adapters;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.Android.Fragments
{
    public class CaseDetailFragment : OpFlowFragmentBase
    {
        private LinearLayout _pnlCaseDetail;
        private TextView _txtSurgeon;
        private TextView _txtCase;
        private TextView _txtReferMD;
        private TextView _txtPatientName;
        private TextView _txtPatientAge;
        private TextView _txtPatientBMI;

        private ExpandableListView _lvCaseDetails;

        private ImageView _ivMedicalHistory;
        private ImageView _ivRiskFactors;
        private ImageView _ivMedications;
        private ImageView _ivAllergies;
        private ImageView _ivLabResults;
        private ImageView _ivPastProcedureResults;
        private ImageView _ivScheduledProcedures;

        private EditText _txtMedicalHistory;
        private EditText _txtRiskFactors;
        private EditText _txtMedications;
        private EditText _txtAllergies;
        private EditText _txtLabResults;
        private EditText _txtPastProcedureResults;
        private GridView _gvScheduledProcedures;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.CaseDetail, container, false);

            _pnlCaseDetail = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCaseDetail);

            _lvCaseDetails = rootView.FindViewById<ExpandableListView>(Resource.Id.lvCaseDetails);

            _txtSurgeon = rootView.FindViewById<TextView>(Resource.Id.txtSurgeon);
            _txtCase = rootView.FindViewById<TextView>(Resource.Id.txtCase);
            _txtReferMD = rootView.FindViewById<TextView>(Resource.Id.txtReferMD);
            _txtPatientName = rootView.FindViewById<TextView>(Resource.Id.txtPatientName);
            _txtPatientAge = rootView.FindViewById<TextView>(Resource.Id.txtPatientAge);
            _txtPatientBMI = rootView.FindViewById<TextView>(Resource.Id.txtPatientBMI);

            _ivMedicalHistory = rootView.FindViewById<ImageView>(Resource.Id.ivMedicalHistory);
            _ivRiskFactors = rootView.FindViewById<ImageView>(Resource.Id.ivRiskFactors);
            _ivMedications = rootView.FindViewById<ImageView>(Resource.Id.ivMedications);
            _ivAllergies = rootView.FindViewById<ImageView>(Resource.Id.ivAllergies);
            _ivLabResults = rootView.FindViewById<ImageView>(Resource.Id.ivLabResults);
            _ivPastProcedureResults = rootView.FindViewById<ImageView>(Resource.Id.ivPastProcedureResults);
            _ivScheduledProcedures = rootView.FindViewById<ImageView>(Resource.Id.ivScheduledProcedures);

            _txtMedicalHistory = rootView.FindViewById<EditText>(Resource.Id.txtMedicalHistory);
            _txtRiskFactors = rootView.FindViewById<EditText>(Resource.Id.txtRiskFactors);
            _txtMedications = rootView.FindViewById<EditText>(Resource.Id.txtMedications);
            _txtAllergies = rootView.FindViewById<EditText>(Resource.Id.txtAllergies);
            _txtLabResults = rootView.FindViewById<EditText>(Resource.Id.txtLabResults);
            _txtPastProcedureResults = rootView.FindViewById<EditText>(Resource.Id.txtPastProcedureResults);
            _gvScheduledProcedures = rootView.FindViewById<GridView>(Resource.Id.gvScheduledProcedures);
            
            var caseDetailTokens = Enum.GetValues(typeof(CaseDetailToken.CaseDetailEnum))
                .Cast<CaseDetailToken.CaseDetailEnum>()
                .Select(value => new CaseDetailToken(value))
                .ToList();

            _lvCaseDetails.SetAdapter(new CaseDetailListAdapter(Activity, caseDetailTokens));

            //_ivMedicalHistory.CheckedChange += Switch_CheckChanged;
            //_ivRiskFactors.CheckedChange += Switch_CheckChanged;
            //_ivMedications.CheckedChange += Switch_CheckChanged;
            //_ivAllergies.CheckedChange += Switch_CheckChanged;
            //_ivLabResults.CheckedChange += Switch_CheckChanged;
            //_ivPastProcedureResults.CheckedChange += Switch_CheckChanged;
            //_ivScheduledProcedures.CheckedChange += Switch_CheckChanged;

            SetSwitchVisibility(_ivMedicalHistory);
            SetSwitchVisibility(_ivRiskFactors);
            SetSwitchVisibility(_ivMedications);
            SetSwitchVisibility(_ivAllergies);
            SetSwitchVisibility(_ivLabResults);
            SetSwitchVisibility(_ivPastProcedureResults);
            SetSwitchVisibility(_ivScheduledProcedures);

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                if (AppSettings.CurrentSurgery != null)
                {
                    await LoadCase(AppSettings.CurrentSurgery.SurgeryID);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void Switch_CheckChanged(object sender, EventArgs e)
        {
            var changedSwitch = (Switch)sender;

            SetSwitchVisibility(null);
        }

        private void SetSwitchVisibility(ImageView changedSection)
        {
            EditText textControl = null;
            GridView gridControl = null;

            if (changedSection == _ivMedicalHistory)
                textControl = _txtMedicalHistory;
            else if (changedSection == _ivRiskFactors)
                textControl = _txtRiskFactors;
            else if (changedSection == _ivMedications)
                textControl = _txtMedications;
            else if (changedSection == _ivAllergies)
                textControl = _txtAllergies;
            else if (changedSection == _ivLabResults)
                textControl = _txtLabResults;
            else if (changedSection == _ivPastProcedureResults)
                textControl = _txtPastProcedureResults;
            else if (changedSection == _ivScheduledProcedures)
                gridControl = _gvScheduledProcedures;

            //if (textControl != null)
            //    textControl.Visibility = (changedSwitch.Checked ? ViewStates.Visible : ViewStates.Gone);
            //if (gridControl != null)
            //    gridControl.Visibility = (changedSwitch.Checked ? ViewStates.Visible : ViewStates.Gone);

        }

        private async Task LoadCase(int surgeryId)
        {
            var locationId = AppSettings.CurrentUser.LocationID;
            var providerId = AppSettings.CurrentUser.ProviderID;

            var currentCase = await SurgeryUtil.GetSurgery(surgeryId, providerId, locationId);
            
            _pnlCaseDetail.Visibility = ViewStates.Visible;

            _txtSurgeon.Text = string.Format("{0}, {1}", currentCase.SurgeonLastName,
                currentCase.SurgeonFirstName);
            _txtCase.Text = currentCase.ProcedureDescription;
            _txtReferMD.Text = currentCase.CardDescription;

            var patient = await PatientUtil.GetPatient(currentCase.PatientID);

            _txtPatientName.Text = string.Format("{0} {1}", patient.FirstName, patient.LastName);
            _txtPatientAge.Text = patient.BirthDate.CalculateAge().ToString();
            _txtPatientBMI.Text = patient.BMI.ToString(CultureInfo.InvariantCulture);


            #region Patient Demo
            var medicalHistory =
                patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.MedicalHistory);
            var riskFactors =
                patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.RiskFactors);
            var medications =
                patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Medications);
            var allergies =
                patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Allergies);
            var labResults =
                patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.LabResults);
            var pastProcedures =
                patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.PastProcedureResult);

            //_swtMedicalHistory.Checked = medicalHistory != null;
            //_swtRiskFactors.Checked = riskFactors != null;
            //_swtMedications.Checked = medications != null;
            //_swtAllergies.Checked = allergies != null;
            //_swtLabResults.Checked = labResults != null;
            //_swtPastProcedureResults.Checked = pastProcedures != null;
            
            _txtMedicalHistory.Text = medicalHistory?.DemoDescription ?? "";
            _txtRiskFactors.Text = riskFactors?.DemoDescription ?? "";
            _txtMedications.Text = medications?.DemoDescription ?? "";
            _txtAllergies.Text = allergies?.DemoDescription ?? "";
            _txtLabResults.Text = labResults?.DemoDescription ?? "";
            _txtPastProcedureResults.Text = pastProcedures?.DemoDescription ?? "";

            #endregion
        }
    }
}