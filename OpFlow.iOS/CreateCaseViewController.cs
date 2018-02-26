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
        private UIPickerView _genderPicker;
        private UIPickerView _specialtyPicker;

        public CreateCaseViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();
            
            SetupDoneStyleTextField(txtCaseNbr, true);
            SetupDoneStyleTextField(txtInitials, true);
            SetupDoneStyleTextField(txtPatientId, true);

            await LoadDropdowns();
        }

        private void GenderPickerRowSelected(object sender, IBindableEntity e)
        {
            txtGender.Text = e.ToString();

            txtGender.ResignFirstResponder();
        }

        private void SpecialtyPickerRowSelected(object sender, IBindableEntity e)
        {
            txtSpecialty.Text = e.ToString();

            txtSpecialty.ResignFirstResponder();
        }

        private async Task LoadDropdowns()
        {
            var genders = await LookupUtil.GetGenders();

            var genderPickerModel = new BasicPickerModel(genders.Cast<IBindableEntity>().ToList());
            genderPickerModel.RowSelected += GenderPickerRowSelected;

            _genderPicker = new UIPickerView();
            _genderPicker.Model = genderPickerModel;
            
            txtGender.InputView = _genderPicker;


            var specialties = await LookupUtil.GetSpecialties();

            var specialtyPickerModel = new BasicPickerModel(specialties.Cast<IBindableEntity>().ToList());
            specialtyPickerModel.RowSelected += SpecialtyPickerRowSelected;

            _specialtyPicker = new UIPickerView();
            _specialtyPicker.Model = specialtyPickerModel;

            txtSpecialty.InputView = _specialtyPicker;
        }
    }
}