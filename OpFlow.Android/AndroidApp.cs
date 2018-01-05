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

namespace OpFlow.Android
{
    public static class AndroidApp
    {
        public const string SURGERY_BUNDLE = "surgery";
        public const string CARD_BUNDLE = "CARD";
        public const string LOCATION_BUNDLE = "LOCATION";
        public const string PROVIDER_BUNDLE = "PROVIDER";
        
        public static Surgery CurrentSurgery;
    }

    public enum FragmentEnum
    {
        Login = 0,
        MainScreen = 1,
        FutureCases = 2,
        Schedule = 3
    }
}