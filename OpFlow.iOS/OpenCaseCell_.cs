using Foundation;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class OpenCaseCell : UITableViewCell
    {
        public OpenCaseCell (IntPtr handle) : base (handle)
        {
        }

        partial void BtnDebrief_TouchUpInside(UIButton sender)
        {
            throw new NotImplementedException();
        }
    }
}