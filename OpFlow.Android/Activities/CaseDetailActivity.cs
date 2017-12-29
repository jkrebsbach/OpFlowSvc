using System;
using System.Collections.Generic;
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

namespace OpFlow.Android.Activities
{
    [Activity(Label = "CaseDetailActivity")]
    public class CaseDetailActivity : Activity
    {
        private LinearLayout _pnlCaseDetail;
        private TextView _txtSurgeon;
        private TextView _txtCase;
        private TextView _txtReferMD;
        private TextView _txtPatientName;
        private TextView _txtPatientAge;
        private TextView _txtPatientBMI;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CaseDetail);

            _pnlCaseDetail = FindViewById<LinearLayout>(Resource.Id.pnlCaseDetail);
            _txtSurgeon = FindViewById<TextView>(Resource.Id.txtSurgeon);
            _txtCase = FindViewById<TextView>(Resource.Id.txtCase);
            _txtReferMD = FindViewById<TextView>(Resource.Id.txtReferMD);
            _txtPatientName = FindViewById<TextView>(Resource.Id.txtPatientName);
            _txtPatientAge = FindViewById<TextView>(Resource.Id.txtPatientAge);
            _txtPatientBMI = FindViewById<TextView>(Resource.Id.txtPatientBMI);

            var caseId = Intent.GetStringExtra(CheckInActivity.CARD_BUNDLE);
            if (!string.IsNullOrEmpty(caseId))
            {
                await LoadCase(int.Parse(caseId));
            }
        }

        private async Task LoadCase(int caseId)
        {
            var currentCase = await CaseUtil.GetCase(caseId);

            _pnlCaseDetail.Visibility = ViewStates.Visible;

            _txtSurgeon.Text = string.Format("{0}, {1}", currentCase.SurgeonLastName,
                currentCase.SurgeonFirstName);
            _txtCase.Text = currentCase.Card?.ProcedureDescription;
            _txtReferMD.Text = currentCase.Card?.CardDescription;

            var patient = await PatientUtil.GetPatient(currentCase.PatientID);

            _txtPatientName.Text = string.Format("{0} {1}", currentCase.PatientFirstName, currentCase.PatientLastName);
            _txtPatientAge.Text = currentCase.PatientBirthDate.CalculateAge().ToString();
            _txtPatientBMI.Text = currentCase.PatientBMI.ToString();

            var medicalHistory =
                patient.DemoData.FirstOrDefault(pd => pd.DataType == PatientDemo.DataTypeEnum.MedicalHistory);
        }
    }
}