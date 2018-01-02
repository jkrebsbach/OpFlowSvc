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

        private Switch _swtMedicalHistory;
        private Switch _swtRiskFactors;
        private Switch _swtMedications;
        private Switch _swtAllergies;
        private Switch _swtLabResults;
        private Switch _swtPastProcedureResults;
        private Switch _swtScheduledProcedures;

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
            _txtSurgeon = rootView.FindViewById<TextView>(Resource.Id.txtSurgeon);
            _txtCase = rootView.FindViewById<TextView>(Resource.Id.txtCase);
            _txtReferMD = rootView.FindViewById<TextView>(Resource.Id.txtReferMD);
            _txtPatientName = rootView.FindViewById<TextView>(Resource.Id.txtPatientName);
            _txtPatientAge = rootView.FindViewById<TextView>(Resource.Id.txtPatientAge);
            _txtPatientBMI = rootView.FindViewById<TextView>(Resource.Id.txtPatientBMI);

            _swtMedicalHistory = rootView.FindViewById<Switch>(Resource.Id.swtMedicalHistory);
            _swtRiskFactors = rootView.FindViewById<Switch>(Resource.Id.swtRiskFactors);
            _swtMedications = rootView.FindViewById<Switch>(Resource.Id.swtMedications);
            _swtAllergies = rootView.FindViewById<Switch>(Resource.Id.swtAllergies);
            _swtLabResults = rootView.FindViewById<Switch>(Resource.Id.swtLabResults);
            _swtPastProcedureResults = rootView.FindViewById<Switch>(Resource.Id.swtPastProcedureResults);
            _swtScheduledProcedures = rootView.FindViewById<Switch>(Resource.Id.swtScheduledProcedures);

            _txtMedicalHistory = rootView.FindViewById<EditText>(Resource.Id.txtMedicalHistory);
            _txtRiskFactors = rootView.FindViewById<EditText>(Resource.Id.txtRiskFactors);
            _txtMedications = rootView.FindViewById<EditText>(Resource.Id.txtMedications);
            _txtAllergies = rootView.FindViewById<EditText>(Resource.Id.txtAllergies);
            _txtLabResults = rootView.FindViewById<EditText>(Resource.Id.txtLabResults);
            _txtPastProcedureResults = rootView.FindViewById<EditText>(Resource.Id.txtPastProcedureResults);
            _gvScheduledProcedures = rootView.FindViewById<GridView>(Resource.Id.gvScheduledProcedures);

            

            _swtMedicalHistory.CheckedChange += Switch_CheckChanged;
            _swtRiskFactors.CheckedChange += Switch_CheckChanged;
            _swtMedications.CheckedChange += Switch_CheckChanged;
            _swtAllergies.CheckedChange += Switch_CheckChanged;
            _swtLabResults.CheckedChange += Switch_CheckChanged;
            _swtPastProcedureResults.CheckedChange += Switch_CheckChanged;
            _swtScheduledProcedures.CheckedChange += Switch_CheckChanged;

            SetSwitchVisibility(_swtMedicalHistory);
            SetSwitchVisibility(_swtRiskFactors);
            SetSwitchVisibility(_swtMedications);
            SetSwitchVisibility(_swtAllergies);
            SetSwitchVisibility(_swtLabResults);
            SetSwitchVisibility(_swtPastProcedureResults);
            SetSwitchVisibility(_swtScheduledProcedures);

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                if (AndroidApp.SurgeryID > 0)
                {
                    await LoadCase(AndroidApp.SurgeryID);
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

            SetSwitchVisibility(changedSwitch);
        }

        private void SetSwitchVisibility(ICheckable changedSwitch)
        {
            EditText textControl = null;
            GridView gridControl = null;

            if (changedSwitch == _swtMedicalHistory)
                textControl = _txtMedicalHistory;
            else if (changedSwitch == _swtRiskFactors)
                textControl = _txtRiskFactors;
            else if (changedSwitch == _swtMedications)
                textControl = _txtMedications;
            else if (changedSwitch == _swtAllergies)
                textControl = _txtAllergies;
            else if (changedSwitch == _swtLabResults)
                textControl = _txtLabResults;
            else if (changedSwitch == _swtPastProcedureResults)
                textControl = _txtPastProcedureResults;
            else if (changedSwitch == _swtScheduledProcedures)
                gridControl = _gvScheduledProcedures;

            if (textControl != null)
                textControl.Visibility = (changedSwitch.Checked ? ViewStates.Visible : ViewStates.Gone);
            if (gridControl != null)
                gridControl.Visibility = (changedSwitch.Checked ? ViewStates.Visible : ViewStates.Gone);

        }

        private async Task LoadCase(int surgeryId)
        {
            var currentCase = await SurgeryUtil.GetSurgery(surgeryId);

            _pnlCaseDetail.Visibility = ViewStates.Visible;

            _txtSurgeon.Text = string.Format("{0}, {1}", currentCase.SurgeonLastName,
                currentCase.SurgeonFirstName);
            _txtCase.Text = currentCase.ProcedureDescription;
            _txtReferMD.Text = currentCase.CardDescription;

            var patient = await PatientUtil.GetPatient(currentCase.PatientID);

            _txtPatientName.Text = string.Format("{0} {1}", currentCase.Patient.FirstName, currentCase.Patient.LastName);
            _txtPatientAge.Text = currentCase.Patient.BirthDate.CalculateAge().ToString();
            _txtPatientBMI.Text = currentCase.Patient.BMI.ToString(CultureInfo.InvariantCulture);


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

            _swtMedicalHistory.Checked = medicalHistory != null;
            _swtRiskFactors.Checked = riskFactors != null;
            _swtMedications.Checked = medications != null;
            _swtAllergies.Checked = allergies != null;
            _swtLabResults.Checked = labResults != null;
            _swtPastProcedureResults.Checked = pastProcedures != null;
            
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