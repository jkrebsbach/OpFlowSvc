using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using OpFlow.Data;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public class RoomSetupSearchTVS : UITableViewSource
    {
        private readonly List<RoomSetup> _roomSetups;

        public RoomSetupSearchTVS(List<RoomSetup> roomSetups)
        {
            _roomSetups = roomSetups;
        }
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var roomSetup = _roomSetups[indexPath.Row];

            var cell = tableView.DequeueReusableCell("RoomSearchResultCell", indexPath) as RoomSearchResultCell;

            cell?.UpdateCell(roomSetup);
            
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _roomSetups.Count;
        }
    }
}