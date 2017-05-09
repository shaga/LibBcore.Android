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
using LibBcore;

namespace LibBcoreTest
{
    class BcoreAdapter : BaseAdapter<BcoreDeviceInfo>
    {
        private readonly Context _context;

        private readonly IList<BcoreDeviceInfo> _list;
        
        public BcoreAdapter(Context context, IList<BcoreDeviceInfo> list)
        {
            _context = context;
            _list = list;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView ?? LayoutInflater.FromContext(_context).Inflate(Android.Resource.Layout.SimpleListItemChecked, null);

            var item = this[position];

            if (item == null) return view;

            var name = view.FindViewById<TextView>(Android.Resource.Id.Text1);
            name.Text = item.Name;

            //var addr = view.FindViewById<TextView>(Android.Resource.Id.Text2);
            //addr.Text = item.Address;

            return view;
        }

        public override int Count => _list?.Count ?? 0;

        public override BcoreDeviceInfo this[int position] => _list != null && 0 <= position && position < Count ? _list[position] : null;
    }
}