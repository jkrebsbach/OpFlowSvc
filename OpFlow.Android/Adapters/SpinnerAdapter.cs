using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using OpFlow.Data;
using Object = Java.Lang.Object;

namespace OpFlow.Android.Adapters
{
    public class SpinnerAdapter<T>
    {
        public List<T> Objects { get; }
        public List<string> DropDownValues { get; }

        public SpinnerAdapter(List<T> sourceData)
        {
            Objects = sourceData;

            DropDownValues = new List<string>();

            foreach (var entity in sourceData)
            {
                DropDownValues.Add(entity.ToString());
            }
        }
    
    }
}