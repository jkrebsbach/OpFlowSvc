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

        private OpFlowTextDatePicker _surgeryDateTime;

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

            txtSurgeon.Enabled = false;
            txtBundle.Enabled = false;

            txtCptCode.ValueChanged += UpdateProcedure;
            
            _surgeryDateTime = new OpFlowTextDatePicker(txtSurgeryDateTime)
            {
                Mode = UIDatePickerMode.DateAndTime
            };

            await LoadDropdowns();
        }

        private async void UpdateSpecialty(object sender, EventArgs e)
        {
            var specialtyId = _specialtyPicker.GetCurrentId();

            if (specialtyId <= 0) return;
            txtSurgeon.Enabled = true;
            txtBundle.Enabled = true;

            var surgeons = await UserUtil.GetSurgeons(specialtyId);
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, surgeons.Cast<IBindableEntity>().ToList());

            var bundles = await LookupUtil.GetBundles(specialtyId);
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
            ShowDialog("DONE", "Done clicked");
        }

        async partial void btnNew_Click(UIKit.UIButton sender)
        {
            ShowDialog("NEW", "New clicked");
        }
    }
}