using Foundation;
using System;
using UIKit;
using OpFlow.Data;

namespace OpFlow.iOS
{
    public partial class SurgeryCommunicatorCell : UITableViewCell
    {
        public SurgeryCommunicatorCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(Messaging message)
        {
            txtCommunicator.Text = message.Message;

            ivCommunicator.SetTitle(message.UserName, UIControlState.Normal);
        }
    }
}