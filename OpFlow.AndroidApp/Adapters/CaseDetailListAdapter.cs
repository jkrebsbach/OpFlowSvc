using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Adapters
{
    public class CaseDetailListAdapter : BaseExpandableListAdapter
    {
        private readonly Activity _context;
        private readonly List<CaseDetailToken> _caseDetailTokens;

        public override int GroupCount => _caseDetailTokens.Count;

        public override bool HasStableIds => true;

        public override Java.Lang.Object GetChild(int groupPosition, int childPosition)
        {
            return null;
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return _caseDetailTokens[groupPosition].DetailId;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return 1;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var row = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.CaseDetailListItem, null);
            var currGroup = _caseDetailTokens[groupPosition];

            var txtItemText = row.FindViewById<TextView>(Resource.Id.txtItemText);
            var pnlScheduledProceduresHeaders = row.FindViewById<LinearLayout>(Resource.Id.pnlScheduledProceduresHeaders);
            var gvScheduledProcedures = row.FindViewById<GridView>(Resource.Id.gvScheduledProcedures);

            txtItemText.Text = currGroup.DetailText;

            //var showPnlPastProcedure = currGroup.CaseDetail == CaseDetailToken.CaseDetailEnum.PastProcedureResults;
            var showPnlPastProcedure = false;
                
            txtItemText.Visibility = !showPnlPastProcedure ? ViewStates.Visible : ViewStates.Gone;
            pnlScheduledProceduresHeaders.Visibility = showPnlPastProcedure ? ViewStates.Visible : ViewStates.Gone;
            gvScheduledProcedures.Visibility = showPnlPastProcedure ? ViewStates.Visible : ViewStates.Gone;

            return row;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return null;
        }

        public override long GetGroupId(int groupPosition)
        {
            return _caseDetailTokens[groupPosition].CategoryGroupId;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var header = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ListGroup, null);

            var token = _caseDetailTokens[groupPosition];

            header.FindViewById<TextView>(Resource.Id.DataHeader).Text = token.CategoryTitle;

            return header;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            return false;
        }

        public CaseDetailListAdapter(Activity context, List<CaseDetailToken> caseDetailTokens)
        {
            _context = context;
            _caseDetailTokens = caseDetailTokens;
        }
    }
}