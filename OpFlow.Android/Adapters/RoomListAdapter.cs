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
    public class RoomListAdapter : BaseAdapter
    {
        private Context _context;
        private List<Room> _rooms;

        public RoomListAdapter(Context c, List<Room> rooms)
        {
            _rooms = rooms;
        }

        public override Object GetItem(int position)
        {
            return null;
        }

        public override long GetItemId(int position)
        {
            return _rooms[position].RoomID;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            return GetStyledText(_rooms[position].RoomDescription);
        }

        private TextView GetStyledText(string content)
        {
            

            TextView text = new TextView(_context)
            { 
                Text = content,
                LayoutParameters = new LinearLayout.LayoutParams(
                    ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent)
                {
                    LeftMargin = 20
                }
            };
            return text;
        }

        public override int Count => _rooms?.Count() ?? 0;
    }
}