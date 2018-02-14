using System;
using System.Collections.Generic;
using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class CommunicatorTVS : UITableViewSource
    {
        private readonly List<Messaging> _messages;

        public event EventHandler<Messaging> SurgerySelectionEvent;
        public event EventHandler<Messaging> DebriefSelectionEvent;

        public CommunicatorTVS(List<Messaging> messages)
        {
            _messages = messages;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var message = _messages[indexPath.Row];
            
            SurgeryCommunicatorCell cell = null;
            cell = tableView.DequeueReusableCell("SurgeryCommunicatorCell", indexPath) as SurgeryCommunicatorCell;
        
            cell?.UpdateCell(message);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
                return _messages.Count;
        }
    }
}
