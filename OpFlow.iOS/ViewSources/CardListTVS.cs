using System;
using System.Collections.Generic;
using Foundation;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS.ViewSources
{
    public partial class CardListTVS : UITableViewSource
    {
        private readonly List<Card> _cards;

        public CardListTVS(List<Card> cards)
        {
            _cards = cards;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var card = _cards[indexPath.Row];

            CardTableCell cell = null;
            cell = tableView.DequeueReusableCell("CardTableCell", indexPath) as CardTableCell;

            cell?.UpdateCell(card);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _cards.Count;
        }
    }
}
