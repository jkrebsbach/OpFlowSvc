using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Foundation;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public class OpFlowTextPicker : UIPickerView
    {
        private readonly UITextField _associatedField;

        public UITextField AssociatedField => _associatedField;

        public int GetCurrentId()
        {
            var selection = (Model as BasicPickerModel)?.CurrentSelection(
                SelectedRowInComponent(0));

            return selection?.GetID() ?? 0;
        }

        [Export("DoneAction")]
        private void DoneAction()
        {
            var selection = (Model as BasicPickerModel)?.CurrentSelection(
                SelectedRowInComponent(0));

            _associatedField.Text = selection?.ToString();
            _associatedField.ResignFirstResponder();
        }

        private void PickerRowSelected(object sender, IBindableEntity e)
        {
            _associatedField.Text = e.ToString();

            _associatedField.ResignFirstResponder();
        }

        public OpFlowTextPicker(UITextField associatedField, List<IBindableEntity> entities) 
        {
            _associatedField = associatedField;
            _associatedField.InputView = this;
            
            var toolBar = new UIToolbar(new CGRect(0, 0, 320, 44));
            var flexibleSpaceLeft = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null, null);
            var doneButton = new UIBarButtonItem("OK", UIBarButtonItemStyle.Done, this, new ObjCRuntime.Selector("DoneAction"));

            var list = new [] { flexibleSpaceLeft, doneButton };
            toolBar.SetItems(list, false);

            //Assign the toolBar to InputAccessoryView 
            _associatedField.InputAccessoryView = toolBar;

            var pickerModel = new BasicPickerModel(entities);

            pickerModel.RowSelected += PickerRowSelected;
            Model = pickerModel;
        }
    }
}
