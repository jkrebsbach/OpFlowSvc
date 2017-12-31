using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
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
            var txtLocation = gridView.FindViewById<TextView>(Resource.Id.txtLocation);
            var txtPatientAge = gridView.FindViewById<TextView>(Resource.Id.txtPatientAge);
            var txtPatientSex = gridView.FindViewById<TextView>(Resource.Id.txtPatientSex);
            var txtProcedure = gridView.FindViewById<TextView>(Resource.Id.txtProcedure);
            var ivPrefCard = gridView.FindViewById<ImageView>(Resource.Id.ivPrefCard);
            var ivAlerts = gridView.FindViewById<ImageView>(Resource.Id.ivAlerts);
            var ivDelays = gridView.FindViewById<ImageView>(Resource.Id.ivDelays);
            var txtDelayAmt = gridView.FindViewById<TextView>(Resource.Id.txtDelayAmt);

            // If there is an estimated delay, highlight the event
            if (schedule.EstDelayMinutes > 0)
            {
                pnlLayout.SetBackgroundResource(Resource.Drawable.HighlightedRoundRectangle);
            }

            txtPatientName.Text = string.Format("{0} {1}", schedule.Patient?.LastName, schedule.Patient?.FirstName);
            txtTime.Text = schedule.ScheduleTime.ToString(@"hh\:mm");
            txtLocation.Text = schedule.ProviderName;
            txtPatientAge.Text = "Age: " + schedule.Patient?.BirthDate.CalculateAge();
            txtPatientSex.Text = "Sex: " + schedule.Patient?.Sex;
            txtProcedure.Text = schedule.ProcedureDescription;

            ivPrefCard.SetImageDrawable(_context.GetDrawable(Resource.Drawable.DarkGreenCheckMark));
            ivAlerts.SetImageDrawable(_context.GetDrawable(Resource.Drawable.DarkGreenCheckMark));
            ivDelays.SetImageDrawable(_context.GetDrawable(
                schedule.EstDelayMinutes == 0 ? 
                    Resource.Drawable.DarkGreenCheckMark : Resource.Drawable.RedExclamationPoint));

            txtDelayAmt.Text = schedule.EstDelayMinutes == 0 ? "" : "+" + schedule.EstDelayMinutes;
            
            return gridView;
        }

        public override int Count => _schedules.Count;
    }
}