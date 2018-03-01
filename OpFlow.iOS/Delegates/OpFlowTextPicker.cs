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

        public EventHandler ValueChanged;

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

            UpdateSelection(selection);
        }

        private void PickerRowSelected(object sender, IBindableEntity e)
        {
            UpdateSelection(e);
        }

        private void UpdateSelection(IBindableEntity selection)
        {
            var text = selection?.ToString();
            if (selection?.GetID() == 0)
                text = string.Empty;

            _associatedField.Text = text;
            _associatedField.ResignFirstResponder();


            ValueChanged?.Invoke(this, null);
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
