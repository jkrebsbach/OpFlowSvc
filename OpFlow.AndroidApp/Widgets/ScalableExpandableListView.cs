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

namespace OpFlow.AndroidApp.Widgets
{
    public class ScalableExpandableListView : ExpandableListView
    {
        protected ScalableExpandableListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public ScalableExpandableListView(Context context) : base(context)
        {
        }

        public ScalableExpandableListView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public ScalableExpandableListView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        public ScalableExpandableListView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            // Calculate entire height by providing a very large height hint.
            // View.MEASURED_SIZE_MASK represents the largest height possible.
            int expandSpec = MeasureSpec.MakeMeasureSpec(View.MeasuredSizeMask, MeasureSpecMode.AtMost);
            base.OnMeasure(widthMeasureSpec, expandSpec);

            var layoutParameters = this.LayoutParameters;
            layoutParameters.Height = this.MeasuredHeight;
        }
    }
}