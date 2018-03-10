using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CreateCaseViewController : OpFlowViewController
    {
        private OpFlowTextPicker _genderPicker;
        private OpFlowTextPicker _specialtyPicker;
        private OpFlowTextPicker _surgeonPicker;
        private OpFlowTextPicker _bundlePicker;

        private OpFlowTextDatePicker _patientDob;
        private OpFlowTextDatePicker _surgeryDate;
        private OpFlowTextDatePicker _surgeryTime;

        public CreateCaseViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupDoneStyleTextField(txtCaseNbr, true);
            SetupDoneStyleTextField(txtInitials, true);
            SetupDoneStyleTextField(txtPatientId, true);

            SetupDoneStyleTextField(txtGender, true);
            SetupDoneStyleTextField(txtSpecialty, true);
            SetupDoneStyleTextField(txtSurgeon, true);

            SetupDoneStyleTextField(txtCptCode, true);

            
            txtCptCode.ValueChanged += UpdateProcedure;
            
            _surgeryDate = new OpFlowTextDatePicker(txtSurgeryDate)
            {
                Mode = UIDatePickerMode.Date
            };
            _surgeryTime = new OpFlowTextDatePicker(txtSurgeryTime)
            {
                Locale = new NSLocale("NL"),
                Mode = UIDatePickerMode.Time
            };
            _patientDob = new OpFlowTextDatePicker(txtPatientDOB)
            {
                Mode = UIDatePickerMode.Date
            };

            await SetupScreen();
        }

        private async Task SetupScreen()
        {
            txtSurgeon.Enabled = false;
            txtBundle.Enabled = false;

            txtCaseNbr.Text = string.Empty;
            txtPatientId.Text = string.Empty;
            txtPatientDOB.Text = string.Empty;
            txtInitials.Text = string.Empty;
            txtGender.Text = string.Empty;

            txtSpecialty.Text = string.Empty;
            txtSurgeon.Text = string.Empty;
            txtBundle.Text = string.Empty;
            txtCptCode.Text = string.Empty;
            txtDefaultCard.Text = string.Empty;
            txtDefaultFlow.Text = string.Empty;
            txtDefaultRoom.Text = string.Empty;

            txtSurgeryDate.Text = DateTime.Today.ToString("MM/dd");
            txtSurgeryTime.Text = DateTime.Today.Add(new TimeSpan(7, 0, 0)).ToString("HH:mm");

            await LoadDropdowns();
        }

        private async void UpdateSpecialty(object sender, EventArgs e)
        {
            var specialtyId = _specialtyPicker.GetCurrentId();

            if (specialtyId <= 0) return;
            txtSurgeon.Enabled = true;
            txtBundle.Enabled = true;

            var surgeons = await UserUtil.GetSurgeons(specialtyId ?? 0);
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, surgeons.Cast<IBindableEntity>().ToList());

            var bundles = await LookupUtil.GetBundles(specialtyId ?? 0);
            _bundlePicker = new OpFlowTextPicker(txtBundle, bundles.Cast<IBindableEntity>().ToList());

            _bundlePicker.ValueChanged += UpdateBundle;
        }

        private async void UpdateBundle(object sender, EventArgs e)
        {
            await UpdateDefaults();
        }

        private async void UpdateProcedure(object sender, EventArgs e)
        {
            await UpdateDefaults();
        }

        private async Task UpdateDefaults()
        {
            var specialtyId = _specialtyPicker?.GetCurrentId();
            var bundleId = _bundlePicker?.GetCurrentId();

            CardFlowRoom cardFlowRoom = null;
            if (bundleId.HasValue)
            {
                cardFlowRoom = await CardUtil.GetBundleDefault(bundleId ?? 0);
            }
            else if (!string.IsNullOrEmpty(txtCptCode.Text))
            {
                cardFlowRoom = await CardUtil.GetProcedureDefault(txtCptCode.Text);

            }

            txtDefaultCard.Text = cardFlowRoom?.CardDescription;
            txtDefaultFlow.Text = cardFlowRoom?.FlowDescription;
            txtDefaultRoom.Text = cardFlowRoom?.RoomDescription;
        }

        private async Task LoadDropdowns()
        {
            var genders = await LookupUtil.GetGenders();
            _genderPicker = new OpFlowTextPicker(txtGender, genders.Cast<IBindableEntity>().ToList());

            var specialties = await LookupUtil.GetSpecialties();
            _specialtyPicker = new OpFlowTextPicker(txtSpecialty, specialties.Cast<IBindableEntity>().ToList());

            _specialtyPicker.ValueChanged += UpdateSpecialty;


        }

        async partial void btnDone_Click(UIKit.UIButton sender)
        {
            var currentData = CurrentData;
            if (currentData == null)
                return;

            var surgeryId = await ExecuteAsyncWebRequest(() => SurgeryUtil.CreateSurgery(currentData));

            if (surgeryId > 0)
            {
                AppSettings.LoadSurgery(surgeryId.Value);
                NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CaseNavigate);
            }
        }

        async partial void btnNew_Click(UIKit.UIButton sender)
        {
            var currentData = CurrentData;
            if (currentData == null)
                return;

            var surgeryId = await ExecuteAsyncWebRequest(() => SurgeryUtil.CreateSurgery(currentData));

            if (surgeryId > 0)
            {
                ShowDialog("NEW", "Surgery created successfully");

                await SetupScreen();
            }
        }

        private SurgeryPost CurrentData
        {
            get
            {
                string errorMessage = null;

                if ((_specialtyPicker?.GetCurrentId() ?? 0) <= 0)
                    errorMessage = "No specialty selected";
                if ((_surgeonPicker?.GetCurrentId() ?? 0) <= 0)
                    errorMessage = "No surgeon selected";
                if (txtCaseNbr.Text == string.Empty)
                    errorMessage = "No Case # provided";
                if (txtPatientId.Text == string.Empty)
                    errorMessage = "No Patient ID provided";
                if (txtInitials.Text == string.Empty)
                    errorMessage = "No Patient Initials provided";
                if (txtGender.Text == string.Empty)
                    errorMessage = "No Patient Gender provided";
                if (txtPatientDOB.Text == string.Empty)
                    errorMessage = "No Patient DOB provided";

                if (errorMessage != null)
                {
                    ShowDialog("ERROR", errorMessage);
                    return null;
                }

                var scheduleDateTime = txtSurgeryDate.Text + " " + txtSurgeryTime.Text;
                var scheduleDate = DateTime.Today;
                DateTime.TryParse(scheduleDateTime, out scheduleDate);

                var surgeryPost = new SurgeryPost()
                {
                     CaseNbr = txtCaseNbr.Text,
                    PtAcctNbr = txtPatientId.Text,
                    PtDOB = DateTime.Parse(txtPatientDOB.Text),
                    PtInitials = txtInitials.Text,
                    PtGender = txtGender.Text,
                    SpecialtyID = _specialtyPicker?.GetCurrentId() ?? 0,
                    SurgeonUserID = _surgeonPicker?.GetCurrentId() ?? 0,
                    BundleID = _bundlePicker?.GetCurrentId(),
                    CptCode = txtCptCode.Text,
                    ScheduleDate = scheduleDate
                };

                return surgeryPost;
            }
        }
    }
}