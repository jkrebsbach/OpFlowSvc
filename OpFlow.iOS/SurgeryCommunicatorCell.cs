using Foundation;
using System;
using UIKit;
using OpFlow.Data;
using OpFlow.iOS.Delegates;

namespace OpFlow.iOS
{
    public partial class SurgeryCommunicatorCell : CommunicationCell
    {
        public SurgeryCommunicatorCell (IntPtr handle) : base (handle)
        {
        }

        protected override UILabel LblCommunicator => lblCommunicator;
        protected override UILabel LblInitials => lblInitials;
    }
}