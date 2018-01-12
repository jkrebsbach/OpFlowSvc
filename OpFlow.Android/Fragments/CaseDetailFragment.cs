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


            var caseDetailTokens = Enum.GetValues(typeof(CaseDetailToken.CaseDetailEnum))
                .Cast<CaseDetailToken.CaseDetailEnum>()
                .Select(value => new CaseDetailToken(patient, value))
                .ToList();

            _lvCaseDetails.SetAdapter(new CaseDetailListAdapter(Activity, caseDetailTokens));

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

            
            #endregion
        }
    }
}