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

namespace OpFlow.AndroidApp.Widgets
{
    public class WeekDayPickerViewChangedArgs
    {
        public WeekDayPickerViewChangedArgs(DateTime date)
        {
            this.Date = date;
        }

        public DateTime Date { get; private set; }
    }
}