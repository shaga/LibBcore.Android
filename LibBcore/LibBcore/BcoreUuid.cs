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
using Java.Util;

namespace LibBcore
{
    class BcoreUuid
    {
        public static readonly UUID BcoreService = UUID.FromString("389CAAF0-843F-4d3b-959D-C954CCE14655");
        public static readonly UUID BcoreServiceRev = UUID.FromString("5546E1CC-54C9-9D95-3B4D-3F84F0AA9C38");
        public static readonly ParcelUuid BcoreScanUuid = new ParcelUuid(BcoreServiceRev);

        public static readonly UUID BatteryVol = UUID.FromString("389CAAF1-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID MotorPwm = UUID.FromString("389CAAF2-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID PortOut = UUID.FromString("389CAAF3-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID ServoPos = UUID.FromString("389CAAF4-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID GetFunctions = UUID.FromString("389CAAFF-843F-4D3B-959D-C954CCE14655");
    }
}