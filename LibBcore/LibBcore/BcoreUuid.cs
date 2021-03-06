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
    internal class BcoreUuid
    {
        public static readonly UUID BcoreService = UUID.FromString("389CAAF0-843F-4D3B-959D-C954CCE14655");
        public static readonly ParcelUuid BcoreScanUuid = new ParcelUuid(BcoreService);

        public static readonly UUID BatteryVol = UUID.FromString("389CAAF1-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID MotorPwm = UUID.FromString("389CAAF2-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID PortOut = UUID.FromString("389CAAF3-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID ServoPos = UUID.FromString("389CAAF4-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID BurstCmd = UUID.FromString("389CAAF5-843F-4D3B-959D-C954CCE14655");
        public static readonly UUID GetFunctions = UUID.FromString("389CAAFF-843F-4D3B-959D-C954CCE14655");
    }
}