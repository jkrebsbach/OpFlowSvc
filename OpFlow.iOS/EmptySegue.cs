using Foundation;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class EmptySegue : UIStoryboardSegue
    {
        public EmptySegue (IntPtr handle) : base (handle)
        {
        }

        public override void Perform()
        {
            // Nothing.  ContainverViewController handles all of the view controller action
        }
    }
}