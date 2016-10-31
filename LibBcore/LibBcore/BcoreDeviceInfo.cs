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
using Object = Java.Lang.Object;

namespace LibBcore
{
    public class BcoreDeviceInfo : Java.Lang.Object
    {
        /// <summary>
        /// Device name of bCore
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Device address of bCore
        /// </summary>
        public string Address { get; }

        public BcoreDeviceInfo(string name, string address)
        {
            Name = name;
            Address = address;
        }

        public override bool Equals(Object o)
        {
            var info = o as BcoreDeviceInfo;

            return info != null && info.Address == Address;
        }
    }
}