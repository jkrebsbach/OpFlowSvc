using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class CommunicationGroupTVS : UITableViewSource
    {
        private readonly List<MessagingGroup> _messageGroups;

        public event EventHandler<MessagingGroup> MessageGroupSelectionEvent;

        public CommunicationGroupTVS(List<MessagingGroup> messageGroups)
        {
            _messageGroups = messageGroups;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var messageGroup = _messageGroups[indexPath.Row];

            CommunicatorGroupCell cell = null;
            cell = tableView.DequeueReusableCell("CommunicatorGroupCell", indexPath) as CommunicatorGroupCell;

            cell?.UpdateCell(messageGroup);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messageGroups.Count;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var messageGroup = _messageGroups[indexPath.Row];

            MessageGroupSelectionEvent?.Invoke(this, messageGroup);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var defaultSize = 50.0f;

            return defaultSize;
        }

    }
}