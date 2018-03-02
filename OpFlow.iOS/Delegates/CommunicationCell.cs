using System;
using System.Collections.Generic;
using System.Text;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using UIKit;

namespace OpFlow.iOS.Delegates
{
    public abstract class CommunicationCell : UITableViewCell
    {
        protected abstract UILabel LblCommunicator { get; }
        protected abstract UILabel LblInitials { get; }

        public CommunicationCell(IntPtr handle) : base(handle)
        {

        }

        public void UpdateCell(Messaging message)
        {
            LblCommunicator.Text = message.Message;
            LblInitials.Text = message.UserName;

            LblInitials.BackgroundColor = message.SenderRoleID.RoleBackgroundColorMapping();
            LblInitials.TextColor = message.SenderRoleID.RoleTextColorMapping();
        }
    }
}
