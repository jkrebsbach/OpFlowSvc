using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Views;
using OpFlow.Android.Activities;
using OpFlow.Mobile;

namespace OpFlow.Android
{
    [Activity(Label = "OpFlow.Android", MainLauncher = true)]
    public class MainActivity : OpFlowActivityBase
    {
        protected override int GetLayoutResourceId()
        {
            return Resource.Layout.Main;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

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

            var btnCases = FindViewById<Button>(Resource.Id.btnCases);
            btnCases.Click += delegate
            {
                var futureCaseActivity = new Intent(this, typeof(FutureCaseActivity));
                StartActivity(futureCaseActivity);
            };
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            // Handle item selection
            switch (item.ItemId)
            {
                case Resource.Id.menu_save:
                    //newGame();
                    return true;
                case Resource.Id.menu_edit:
                    //showHelp();
                    return true;
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        //protected override void OnResume()
        //{
        //    AuthenticateUser();

        //    base.OnResume();
        //}

        private void AuthenticateUser()
        {
            try
            {
                if (!AppSettings.UserAuthenticated)
                {
                    var loginActivity = new Intent(this, typeof(LoginActivity));
                    StartActivity(loginActivity);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }
    }
}

