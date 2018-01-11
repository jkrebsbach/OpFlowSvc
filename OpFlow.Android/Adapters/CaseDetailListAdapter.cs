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

namespace OpFlow.Android.Adapters
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
            return (long)_caseDetailTokens[groupPosition].CaseDetail;
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return 1;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var row = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.CaseDetailListItem, null);

            string newId = "ABC", newValue = "XYZ";
            
            row.FindViewById<TextView>(Resource.Id.DataId).Text = newId;
            row.FindViewById<TextView>(Resource.Id.DataValue).Text = newValue;

            return row;
        }

        public override Java.Lang.Object GetGroup(int groupPosition)
        {
            return null;
        }

        public override long GetGroupId(int groupPosition)
        {
            return (long)_caseDetailTokens[groupPosition].CaseDetail;
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var header = convertView ?? _context.LayoutInflater.Inflate(Resource.Layout.ListGroup, null);

            var token = _caseDetailTokens[groupPosition];

            header.FindViewById<TextView>(Resource.Id.DataHeader).Text = token.CaseDetail.ToString();

            return header;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            throw new NotImplementedException();
        }

        public CaseDetailListAdapter(Activity context, List<CaseDetailToken> caseDetailTokens)
        {
            _context = context;
            _caseDetailTokens = caseDetailTokens;
        }
    }
}