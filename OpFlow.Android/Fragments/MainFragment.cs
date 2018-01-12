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
using OpFlow.Android.Activities;
using OpFlow.Mobile;

namespace OpFlow.Android.Fragments
{
    public class MainFragment : OpFlowFragmentBase
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.HomeScreen, container, false);

            //var btnCheckIn = rootView.FindViewById<Button>(Resource.Id.btnCheckIn);
            //btnCheckIn.Click += delegate
            //{
            //    Listener.SendMessage(AppSettings.FragmentEnum.MainScreen, new CheckInFragment());
            //};

            //var btnSchedule = rootView.FindViewById<Button>(Resource.Id.btnSchedule);
            //btnSchedule.Click += delegate
            //{
            //    Listener.SendMessage(AppSettings.FragmentEnum.MainScreen, new ScheduleFragment());
            //};

            //var btnCases = rootView.FindViewById<Button>(Resource.Id.btnCases);
            //btnCases.Click += delegate
            //{
            //    Listener.SendMessage(AppSettings.FragmentEnum.MainScreen, new FutureCaseFragment());
            //};

            return rootView;
        }
    }
}