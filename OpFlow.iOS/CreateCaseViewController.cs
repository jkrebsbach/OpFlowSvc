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

            txtSpecialty.ValueChanged += UpdateSpecialty;
            txtSurgeon.Enabled = false;

            await LoadDropdowns();
        }

        private async void UpdateSpecialty(object sender, EventArgs e)
        {
            var specialtyId = _specialtyPicker.GetCurrentId();

            var surgeons = await UserUtil.GetSurgeons(specialtyId);
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, surgeons.Cast<IBindableEntity>().ToList());

        }

        private async Task LoadDropdowns()
        {
            var genders = await LookupUtil.GetGenders();
            _genderPicker = new OpFlowTextPicker(txtGender, genders.Cast<IBindableEntity>().ToList());

            var specialties = await LookupUtil.GetSpecialties();
            _specialtyPicker = new OpFlowTextPicker(txtSpecialty, specialties.Cast<IBindableEntity>().ToList());
        }
    }
}