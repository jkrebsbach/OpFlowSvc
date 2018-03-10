using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using OpFlow.iOS.ViewSources;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public class OpFlowTextDatePicker : UIDatePicker
    {
        private readonly UITextField _associatedField;

        public UITextField AssociatedField => _associatedField;

        public EventHandler DateChanged;


        [Export("DoneAction")]
        private void DoneAction()
        {
            var formatter = new NSDateFormatter();
            switch (Mode)
            {
                case UIDatePickerMode.DateAndTime:
                    formatter.DateFormat = "MMM d HH:mm tt";
                    break;
                case UIDatePickerMode.Time:
                    formatter.DateFormat = "HH:mm tt";
                    break;
                default:
                    formatter.DateFormat = "M/d/yyyy";
                    break;
            }
            
            _associatedField.Text = formatter.ToString(Date);
            _associatedField.ResignFirstResponder();

            DateChanged?.Invoke(this, null);
        }

        public OpFlowTextDatePicker(UITextField associatedField)
        {
            _associatedField = associatedField;
            _associatedField.InputView = this;

            var toolBar = new UIToolbar(new CGRect(0, 0, 320, 44));
            var flexibleSpaceLeft = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace, null, null);
            var doneButton = new UIBarButtonItem("OK", UIBarButtonItemStyle.Done, this, new ObjCRuntime.Selector("DoneAction"));

            var list = new[] { flexibleSpaceLeft, doneButton };
            toolBar.SetItems(list, false);

            //Assign the toolBar to InputAccessoryView 
            _associatedField.InputAccessoryView = toolBar;
        }
    }
}