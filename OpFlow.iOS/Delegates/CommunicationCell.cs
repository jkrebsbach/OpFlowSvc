using System;
using System.Collections.Generic;
using System.Text;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class CommunicationCell : UITableViewCell
    {
        protected abstract UILabel LblCommunicator { get; }
        protected abstract UILabel LblInitials { get; }
        protected abstract UIImageView IvCommunicator { get; }

        public CommunicationCell(IntPtr handle) : base(handle)
        {

        }

        public void UpdateCell(Messaging message)
        {
            LblCommunicator.Text = message.Message;
            LblInitials.Text = message.UserName;

            IvCommunicator.AccessibilityLabel = message.UserName;
        }
    }
}
