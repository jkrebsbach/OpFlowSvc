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

            var messageTimestamp = messageGroup.LatestInsertTimestamp.ToString("HH:mm");
            if (messageGroup.LatestInsertTimestamp.Date != DateTime.Today)
                messageTimestamp = messageGroup.LatestInsertTimestamp.ToString("M/d");

            lblRecentMessage.Text = (messageGroup.SenderUserID == AppSettings.CurrentUser.UserID ? "You: " : " ") +
                messageTimestamp + " " +
                messageGroup.LatestMessage;
            
            var backgroundColor = UIColor.FromRGB(100, 149, 237);

            if (messageGroup.CommunicationUserID.HasValue)
            {
                backgroundColor = UIColor.Green;
            }

            lblMessageGroup.BackgroundColor = backgroundColor;
        }
    }
}