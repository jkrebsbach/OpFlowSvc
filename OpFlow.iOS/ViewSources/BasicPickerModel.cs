using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class BasicPickerModel : UIPickerViewModel
    {
        private readonly List<IBindableEntity> _entities;

        public EventHandler<IBindableEntity> RowSelected;

        public BasicPickerModel(List<IBindableEntity> entities)
        {
            _entities = entities;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _entities.Count;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return _entities[(int)row].ToString();
        }

        private IBindableEntity GetEntity(nint row)
        {
            if (_entities == null || _entities.Count <= (int) row)
                return null;

            return _entities[(int)row];
            
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            var selection = GetEntity(row);

            RowSelected?.Invoke(this, selection);
        }

        public IBindableEntity CurrentSelection(nint row)
        {
            var selection = GetEntity(row);

            return selection;
        }
    }
}