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
    [Activity(Label = "OpFlow.Android", MainLauncher = true)]
    public class MainActivity : OpFlowActivityBase, IFragmentMessageListener
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
            if (!AppSettings.UserAuthenticated)
                return new LoginFragment();

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

            return new MainFragment();
        }

        //protected override void OnResume()
        //{
        //    AuthenticateUser();

        //    base.OnResume();
        //}

        private Fragment AuthenticateUser()
        {
            try
            {
                if (!AppSettings.UserAuthenticated)
                {
                    return new LoginFragment();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }

        public void SendMessage(FragmentEnum fragment, object payload)
        {
            Fragment newFragment = null;
            if (fragment == FragmentEnum.Login)
            {
                newFragment = new MainFragment();
            }
            else if (fragment == FragmentEnum.MainScreen)
            {
                newFragment = (Fragment) payload;
            }
            else if (fragment == FragmentEnum.FutureCases)
            {
                AndroidApp.CurrentSurgery = (Surgery)payload;
                newFragment = new CaseDetailFragment();
            }
            else if (fragment == FragmentEnum.Schedule)
            {
                AndroidApp.CurrentSurgery = (Surgery) payload;
                newFragment = new CheckInFragment();
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

