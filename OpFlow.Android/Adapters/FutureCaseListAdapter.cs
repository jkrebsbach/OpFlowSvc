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
    public class FutureCaseListAdapter : BaseAdapter
    {
        private Context _context;
        private List<Schedule> _cases;

        public FutureCaseListAdapter(Context c, List<Schedule> cases)
        {
            _context = c;
            _cases = cases;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return _cases[position].CardID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            var gridView = convertView;
            if (convertView == null)
            {
                gridView = inflater.Inflate(Resource.Layout.FutureCaseListItem, parent, false);
            }

            var currentCase = _cases[position];

            var txtPatientName = gridView.FindViewById<TextView>(Resource.Id.txtPatientName);
            var txtProcedure = gridView.FindViewById<TextView>(Resource.Id.txtProcedure);
            var txtCaseTime = gridView.FindViewById<TextView>(Resource.Id.txtCaseTime);
            var ivStatus = gridView.FindViewById<ImageView>(Resource.Id.ivStatus);

            txtPatientName.Text = currentCase.PatientLastName;
            txtProcedure.Text = currentCase.ScheduleProcedure;
            txtCaseTime.Text = currentCase.ScheduleTime.ToLongDateString();

            // TODO: Make this dependent on status of schedule event
            ivStatus.SetImageDrawable(_context.GetDrawable(Resource.Drawable.DarkGreenCheckMark));
            
            return gridView;
        }

        public override int Count => _cases.Count;
    }
}