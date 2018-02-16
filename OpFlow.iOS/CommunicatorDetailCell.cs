using Foundation;
using System;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CommunicatorDetailCell : CommunicationCell
    {
        public CommunicatorDetailCell (IntPtr handle) : base (handle)
        {
        }

        protected override UILabel LblCommunicator => lblCommunicator;

        protected override UIImageView IvCommunicator => ivCommunicator;


    }
}