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
using OpFlow.AndroidApp.Fragments;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Activities
{
    public abstract class OpFlowActivityBase : Activity, IFragmentMessageListener
    {
        private Toolbar _toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(GetLayoutResourceId());

            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);

            //toolbar.MenuItemClick += OnMenuItemClick;

            //ActionBar.Title = "";

            
        }

        public void UpdateToolbar()
        {
            var appName = _toolbar.FindViewById<TextView>(Resource.Id.txtHeaderApplication);
            var userName = _toolbar.FindViewById<TextView>(Resource.Id.txtHeaderUser);
            var appIcon = _toolbar.FindViewById<ImageView>(Resource.Id.ivHeaderIcon);

            var currentUser = AppSettings.CurrentUser;

            appName.Text = AppSettings.CurrentScreenName;
            userName.Text = $"{currentUser?.Title} {currentUser?.LastName}";

            var appIconResource = Resource.Drawable.ic_search_grey_50_18dp;
            switch (AppSettings.CurrentScreen)
            {
                case AppSettings.FragmentEnum.CaseDetail:
                    appIconResource = Resource.Drawable.ic_work_grey_50_18dp;
                    break;
                case AppSettings.FragmentEnum.Schedule:
                case AppSettings.FragmentEnum.CaseNavigate:
                    appIconResource = Resource.Drawable.ic_schedule_grey_50_18dp;
                    break;
                case AppSettings.FragmentEnum.FutureCases:
                    appIconResource = Resource.Drawable.ic_work_grey_50_18dp;
                    break;
                case AppSettings.FragmentEnum.CheckIn:
                    appIconResource = Resource.Drawable.ic_work_grey_50_18dp;
                    break;
            }

            appIcon.SetImageResource(appIconResource);
        }

        public void SendMessage(AppSettings.FragmentEnum fragment, object payload)
        {
            Fragment newFragment = null;
            if (fragment == AppSettings.FragmentEnum.FutureCases)
            {
                newFragment = new CaseDetailFragment();
            }
            else if (fragment == AppSettings.FragmentEnum.Schedule)
            {
                newFragment = new CaseNavigateFragment();
            }
            else if (fragment == AppSettings.FragmentEnum.CaseNavigate)
            {
                var targetScene = (AppSettings.FragmentEnum) payload;

                if (targetScene == AppSettings.FragmentEnum.CheckIn)
                    newFragment = new CheckInFragment();
                if (targetScene == AppSettings.FragmentEnum.CaseDetail)
                    newFragment = new CaseDetailFragment();
                if (targetScene == AppSettings.FragmentEnum.Flow)
                    newFragment = new FlowFragment();
                if (targetScene == AppSettings.FragmentEnum.Room)
                    newFragment = new RoomFragment();
            }

            if (newFragment != null)
            {

                if (!IsFinishing)
                {
                    FragmentManager.BeginTransaction()
                        .Replace(Resource.Id.mainFragment, newFragment)
                        .AddToBackStack(null)
                        .Commit();
                }
            }
        }
        

        protected abstract int GetLayoutResourceId();
    }
}
