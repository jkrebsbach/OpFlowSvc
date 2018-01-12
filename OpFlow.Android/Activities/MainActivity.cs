using System;
using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Android.Views;
using OpFlow.Android.Activities;
using OpFlow.Android.Fragments;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.Android
{
    [Activity(Label = "OpFlow", MainLauncher = true)]
    public class MainActivity : OpFlowActivityBase
    {
        protected override int GetLayoutResourceId()
        {
            return Resource.Layout.Main;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            FragmentManager.BeginTransaction()
                .Replace(Resource.Id.mainFragment, InitializeFragment())
                .Commit();
        }

        private Fragment InitializeFragment()
        {
            //if (QAMobile.getInstance().CurrentApp == null)
            //{
            //    return new UserLoginFragment();
            //}
            //else if (QAMobile.getInstance().CurrentApp == QAMobile.ApplicationsEnum.ResawList || QAMobile.getInstance().CurrentApp == QAMobile.ApplicationsEnum.ResawCutNotChecked)
            //{
            //    return new ResawReportFragment();
            //}
            //else if (QAMobile.getInstance().CurrentApp == QAMobile.ApplicationsEnum.PostStripPatchList
            //         || QAMobile.getInstance().CurrentApp == QAMobile.ApplicationsEnum.PostStripDailyPatching
            //         || QAMobile.getInstance().CurrentApp == QAMobile.ApplicationsEnum.PostStripQAPatchCheck)
            //{
            //    return new PatchingReportFragment();
            //}
            //else
            //{
            //    return new SelectCastFragment();
            //}

            return new ScheduleFragment();
        }

        protected override void OnResume()
        {
            AuthenticateUser();

            base.OnResume();
        }

        private Fragment AuthenticateUser()
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

            return null;
        }

        protected override void NavbarClicked(IMenuItem item)
        {
            Fragment newFragment = null;
            if (item.ItemId == Resource.Id.menu_search)
            {
                newFragment = new CaseDetailFragment();
            }
            else if (item.ItemId == Resource.Id.menu_schedule)
            {
                newFragment = new ScheduleFragment();
            }
            else if (item.ItemId == Resource.Id.menu_cases)
            {
                newFragment = new FutureCaseFragment();
            }
            else if (item.ItemId == Resource.Id.menu_communicator)
            {
                newFragment = new FutureCaseFragment();
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
    }
}

