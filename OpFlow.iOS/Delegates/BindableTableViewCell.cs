using System;
using System.Collections.Generic;
using System.Text;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class BindableTableViewCell : UITableViewCell
    {
        public BindableTableViewCell(IntPtr handle) : base(handle)
        {
        }

        public abstract void UpdateCell(IBindableEntity entity);
    }
}
