using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Adapters
{
    public class ScheduleGridAdapter : BaseAdapter
    {
        private Context _context;
        private List<SurgerySchedule> _schedule;
        private Dictionary<int, Patient> _surgeryPatients;
     
        public ScheduleGridAdapter(Context c, List<SurgerySchedule> schedule, Dictionary<int, Patient> surgeryPatients)
        {
            _context = c;
            _schedule = schedule;
            _surgeryPatients = surgeryPatients;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return _schedule[position].SurgeryID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var inflater = (LayoutInflater)_context.GetSystemService(Context.LayoutInflaterService);
            var gridView = convertView;
            if (convertView == null)
            {
                gridView = inflater.Inflate(Resource.Layout.ScheduleGrid, parent, false);
            }

            var surgery = _schedule[position];
            var patient = _surgeryPatients[surgery.SurgeryID];

            var pnlLayout = gridView.FindViewById<LinearLayout>(Resource.Id.pnlLayout);
            var txtPatientName = gridView.FindViewById<TextView>(Resource.Id.txtPatientName);
            var txtTime = gridView.FindViewById<TextView>(Resource.Id.txtTime);
            var txtLocation = gridView.FindViewById<TextView>(Resource.Id.txtLocation);
            var txtPatientAge = gridView.FindViewById<TextView>(Resource.Id.txtPatientAge);
            var txtPatientSex = gridView.FindViewById<TextView>(Resource.Id.txtPatientSex);
            var txtProcedure = gridView.FindViewById<TextView>(Resource.Id.txtProcedure);
            var txtFlowStep = gridView.FindViewById<TextView>(Resource.Id.txtFlowStep);
            var ivPrefCard = gridView.FindViewById<ImageView>(Resource.Id.ivPrefCard);
            var ivAlerts = gridView.FindViewById<ImageView>(Resource.Id.ivAlerts);
            var ivDelays = gridView.FindViewById<ImageView>(Resource.Id.ivDelays);
            var txtDelayAmt = gridView.FindViewById<TextView>(Resource.Id.txtDelayAmt);
            
            var pnlScheduledCase = gridView.FindViewById<LinearLayout>(Resource.Id.pnlScheduledCase);
            var pnlOpenCase = gridView.FindViewById<LinearLayout>(Resource.Id.pnlOpenCase);

            switch (surgery.SurgeryStatus)
            {
                case "A":
                    pnlScheduledCase.Visibility = ViewStates.Visible;
                    pnlOpenCase.Visibility = ViewStates.Gone;

                    gridView.SetBackgroundResource(Resource.Drawable.RoundRectangle);
                    break;
                case "O":
                    pnlScheduledCase.Visibility = ViewStates.Gone;
                    pnlOpenCase.Visibility = ViewStates.Visible;

                    gridView.SetBackgroundResource(Resource.Drawable.RoundGrayRectangle);
                    break;
            }
            // If there is an estimated delay, highlight the event
            if (surgery.EstDelayMinutes > 0)
            {
                pnlLayout.SetBackgroundResource(Resource.Drawable.HighlightedRoundRectangle);
            }

            txtPatientName.Text = $"{patient?.LastName} {patient?.FirstName}";
            txtTime.Text = surgery.ScheduleTime.ToString(@"hh\:mm");
            txtLocation.Text = surgery.RoomDescription;
            txtPatientAge.Text = patient?.BirthDate.CalculateAge().ToString();
            txtPatientSex.Text = patient?.Gender;
            txtProcedure.Text = surgery.ProcedureDescription;

            txtFlowStep.Text = string.Format("Flow Step: {0}", surgery.FlowStepDescription);

            ivPrefCard.SetImageDrawable(_context.GetDrawable(Resource.Drawable.DarkGreenCheckMark));
            ivAlerts.SetImageDrawable(_context.GetDrawable(Resource.Drawable.DarkGreenCheckMark));
            ivDelays.SetImageDrawable(_context.GetDrawable(
                surgery.EstDelayMinutes == 0 ? 
                    Resource.Drawable.DarkGreenCheckMark : Resource.Drawable.RedExclamationPoint));

            txtDelayAmt.Text = surgery.EstDelayMinutes == 0 ? "" : "+" + surgery.EstDelayMinutes;
            
            return gridView;
        }

        public override int Count => _schedule.Count;
    }
}