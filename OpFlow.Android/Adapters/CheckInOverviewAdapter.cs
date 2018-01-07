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
using OpFlow.Data;

namespace OpFlow.Android.Adapters
{
    public class CheckInOverviewAdapter : BaseAdapter
    {
        private Context _context;
        private List<SurgeryUser> _surgeryUsers;

        public CheckInOverviewAdapter(Context c, List<SurgeryUser> surgeryUsers)
        {
            _context = c;
            _surgeryUsers = surgeryUsers;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return _surgeryUsers[position].UserID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            var gridView = convertView;
            if (convertView == null)
            {
                gridView = inflater.Inflate(Resource.Layout.CheckInOverviewGrid, parent, false);
            }

            var surgeryUser = _surgeryUsers[position];

            var txtRole = gridView.FindViewById<TextView>(Resource.Id.txtRole);
            var ivRoleStatus = gridView.FindViewById<ImageView>(Resource.Id.ivRoleStatus);
            var txtDelayStatus = gridView.FindViewById<TextView>(Resource.Id.txtDelayStatus);
            var ivReviewStatus = gridView.FindViewById<ImageView>(Resource.Id.ivReviewStatus);
            
            txtRole.Text = surgeryUser.RoleDescription;
            ivRoleStatus.SetImageDrawable(
                surgeryUser.CheckInTime.HasValue ?
                _context.GetDrawable(Resource.Drawable.DarkGreenCheckMark) :
                    _context.GetDrawable(Resource.Drawable.RedExclamationPoint));
            txtDelayStatus.Text = "+ 75 mins";
            ivReviewStatus.SetImageDrawable(
                string.IsNullOrEmpty(surgeryUser.WorkupReview) ?
                null :
                _context.GetDrawable(Resource.Drawable.DarkGreenCheckMark));

            return gridView;
        }

        public override int Count => _surgeryUsers.Count;
    }
}