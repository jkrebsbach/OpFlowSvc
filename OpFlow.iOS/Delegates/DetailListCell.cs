using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class DetailListCell : UITableViewCell
    {
        public DetailListCell(IntPtr handle) : base(handle)
        { }

        public abstract void UpdateCell(CaseDetailToken token);
    }
}