using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Mobile;

namespace OpFlow.Android.Activities
{
    [Activity(Label = "ScheduleActivity")]
    public class ScheduleActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Schedule);

            await SetupScreen();
        }

        private async Task SetupScreen()
        {
            var schedule = await WebUtility.GetSchedules();
        }
    }
}