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
using OpFlow.Mobile;

namespace OpFlow.Android.Adapters
{
    public class ScheduleGridAdapter : BaseAdapter
    {
        private Context _context;
        private List<Schedule> _schedules;

        public ScheduleGridAdapter(Context c, List<Schedule> schedules)
        {
            _context = c;
            _schedules = schedules;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return _schedules[position].SurgeryID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            var gridView = convertView;
            if (convertView == null)
            {
                gridView = inflater.Inflate(Resource.Layout.ScheduleGrid, parent, false);
            }

            var schedule = _schedules[position];

            var pnlLayout = gridView.FindViewById<LinearLayout>(Resource.Id.pnlLayout);
            var txtPatientName = gridView.FindViewById<TextView>(Resource.Id.txtPatientName);
            var txtTime = gridView.FindViewById<TextView>(Resource.Id.txtTime);
            var txtCode = gridView.FindViewById<TextView>(Resource.Id.txtCode);
            var txtPatientAge = gridView.FindViewById<TextView>(Resource.Id.txtPatientAge);
            var txtPatientSex = gridView.FindViewById<TextView>(Resource.Id.txtPatientSex);
            var txtProcedure = gridView.FindViewById<TextView>(Resource.Id.txtProcedure);

            // If the case is out of tolerance, highlight the event
            if (schedule.SurgeryID % 2 == 0)
            {
                pnlLayout.SetBackgroundResource(Resource.Drawable.HighlightedRoundRectangle);
            }

            txtPatientName.Text = string.Format("{0} {1}", schedule.PatientLastName, schedule.PatientFirstName);
            txtTime.Text = schedule.ScheduleTime.ToString("H:mm tt");
            txtCode.Text = schedule.ScheduleProcedure;
            txtPatientAge.Text = "Age: " + schedule.PatientBirthDate.CalculateAge();
            txtPatientSex.Text = "Sex: " + schedule.PatientSex;
            txtProcedure.Text = schedule.ScheduleProcedure;
            
            return gridView;
        }

        public override int Count => _schedules.Count;
    }
}