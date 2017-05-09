using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ScanMode = Android.Bluetooth.LE.ScanMode;

namespace LibBcore
{
    public class BcoreScanner
    {
        #region inner class

        /// <summary>
        /// Scan callback before KitKat
        /// </summary>
        public class LeScanCallback : Java.Lang.Object, BluetoothAdapter.ILeScanCallback
        {
            public event EventHandler<BluetoothDevice> FoundBcore;

            public void OnLeScan(BluetoothDevice device, int rssi, byte[] scanRecord)
            {
                if (IsDeviceBcore(scanRecord)) FoundBcore?.Invoke(this, device);
            }

            /// <summary>
            /// check scan record include bCore Service
            /// </summary>
            /// <param name="scanRecord">Advertising packet</param>
            /// <returns></returns>
            private bool IsDeviceBcore(byte[] scanRecord)
            {
                if (scanRecord == null) return false;

                var pos = 0;

                while (pos < scanRecord.Length - 2)
                {
                    var len = scanRecord[pos++];
                    var type = scanRecord[pos];

                    if ((type == 6 || type == 7) && len == 17)
                    {
                        var uuid = string.Empty;

                        for (var i = 1; i <= 16; i++)
                        {
                            uuid += scanRecord[pos + len - i].ToString("X2");
                            if (i == 4 || i == 6 || i == 8 || i == 10)
                            {
                                uuid += "-";
                            }
                        }

                        if (string.Compare(uuid, BcoreUuid.BcoreService.ToString(),
                            StringComparison.OrdinalIgnoreCase) == 0) return true;
                    }

                    pos += len;
                }

                return false;
            }
        }

        /// <summary>
        /// Scan callback after Lollipop
        /// </summary>
        public class ScanCallback : Android.Bluetooth.LE.ScanCallback
        {
            public event EventHandler<BluetoothDevice> FoundBcore;

            public override void OnScanResult(ScanCallbackType callbackType, ScanResult result)
            {
                base.OnScanResult(callbackType, result);

                FoundBcore?.Invoke(this, result.Device);
            }
        }

        #endregion

        #region property

        /// <summary>
        /// Scanning bCore
        /// </summary>
        public bool IsScanning { get; set; }

        private static bool IsAfterLollipop => Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;

        private BluetoothManager BluetoothManager { get; }

        private BluetoothAdapter BluetoothAdapter => BluetoothManager?.Adapter;

        private LeScanCallback LeCallback { get; set; }

        private BluetoothLeScanner Scanner => BluetoothAdapter?.BluetoothLeScanner;

        private ScanCallback Callback { get; set; }

        private IList<ScanFilter> ScanFilters { get; set; }

        private ScanSettings ScanSettings { get; set; }

        #endregion

        #region events

        /// <summary>
        /// event for found bCore
        /// </summary>
        public event EventHandler<BcoreDeviceInfo> FoundBcore;

        #endregion

        #region constructor

        /// <summary>
        /// BcoreScanner constuctor
        /// </summary>
        /// <param name="context">using context</param>
        public BcoreScanner(Context context)
        {
            BluetoothManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;

            if (IsAfterLollipop) InitScanner();
            else InitLeScanner();
        }

        #endregion

        #region method

        #region public

        /// <summary>
        /// Start scan bCore.
        /// </summary>
        public void StartScan()
        {
            if (IsScanning) return;

            IsScanning = true;

            if (IsAfterLollipop)
            {
                Scanner.StartScan(ScanFilters, ScanSettings, Callback);
            }
            else
            {
                BluetoothAdapter.StartLeScan(LeCallback);
            }
        }

        /// <summary>
        /// Stop scan bCore.
        /// </summary>
        public void StopScan()
        {
            if (!IsScanning) return;

            if (IsAfterLollipop)
            {
                Scanner.StopScan(Callback);
            }
            else
            {
                BluetoothAdapter.StopLeScan(LeCallback);
            }

            IsScanning = false;
        }

        #endregion

        #region private

        /// <summary>
        /// Initialize Scanner after Lollipop
        /// </summary>
        private void InitScanner()
        {
            ScanFilters = new List<ScanFilter>
            {
                new ScanFilter.Builder().SetServiceUuid(BcoreUuid.BcoreScanUuid).Build()
            };

            ScanSettings = new ScanSettings.Builder().SetScanMode(ScanMode.Balanced).Build();

            Callback = new ScanCallback();
            Callback.FoundBcore += OnFoundBcore;
        }

        /// <summary>
        /// Initialize LeScanner before KitKat
        /// </summary>
        private void InitLeScanner()
        {
            LeCallback = new LeScanCallback();
            LeCallback.FoundBcore += OnFoundBcore;
        }

        private void OnFoundBcore(object sender, BluetoothDevice device)
        {
            FoundBcore?.Invoke(this, new BcoreDeviceInfo(device.Name, device.Address));
        }

        #endregion

        #endregion
    }
}