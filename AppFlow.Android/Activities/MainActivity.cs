using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using OpFlow.Android.Activities;

namespace OpFlow.Android
{
    [Activity(Label = "OpFlow.Android", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            
            var btnCheckIn = FindViewById<Button>(Resource.Id.btnCheckIn);
            btnCheckIn.Click += delegate
            {
                var checkInActivity = new Intent(this, typeof(CheckInActivity));
                StartActivity(checkInActivity);
            };

            var btnSchedule = FindViewById<Button>(Resource.Id.btnSchedule);
            btnSchedule.Click += delegate
            {
                var scheduleActivity = new Intent(this, typeof(ScheduleActivity));
                StartActivity(scheduleActivity);
            };
        }
    }
}

