using Foundation;
using System;
using OpFlow.Data;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CommunicatorGroupCell : UITableViewCell
    {
        public CommunicatorGroupCell (IntPtr handle) : base (handle)
        {
        }

        public void UpdateCell(MessagingGroup messageGroup)
        {
            lblMessageGroup.Text = messageGroup.CommunicationTargetName;

            lblRecentMessage.Text = (messageGroup.SenderUserID == AppSettings.CurrentUser.UserID ? "You: " : " ") +
                messageGroup.LatestMessage;

            var backgroundColor = UIColor.Green;

            if (messageGroup.CommunicationUserID.HasValue)
            {
                backgroundColor = UIColor.FromRGB(100, 149, 237);
            }

            //Layer.BackgroundColor = backgroundColor.CGColor;
            lblMessageGroup.BackgroundColor = backgroundColor;
        }
    }
}