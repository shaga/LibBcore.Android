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

namespace LibBcore
{
    internal static class BcoreIntent
    {
        public static string GetAddress(this Intent intent)
        {
            return intent.GetStringExtra(BcoreStatusReceiver.ExtraKeyAddress);
        }

        public static EBcoreConnectionState GetConnectionState(this Intent intent)
        {
            var state = intent.GetIntExtra(BcoreStatusReceiver.ExtraKeyConnectionStatus,
                (int) EBcoreConnectionState.Disconnected);

            return (EBcoreConnectionState) state;
        }

        public static bool GetDiscoveredServiceResult(this Intent intent)
        {
            return intent.GetBooleanExtra(BcoreStatusReceiver.ExtraKeyIsDiscoveredService, false);
        }

        public static int GetBatteryVoltage(this Intent intent)
        {
            return intent.GetIntExtra(BcoreStatusReceiver.ExtraKeyBatteryVoltage, 0);
        }

        public static BcoreFunctionInfo GetFunctionInfo(this Intent intent)
        {
            var value = intent.GetByteArrayExtra(BcoreStatusReceiver.ExtraKeyFunctions);

            if (value == null) return null;

            return new BcoreFunctionInfo(value);
        }
    }

    [BroadcastReceiver]
    public class BcoreStatusReceiver : BroadcastReceiver
    {
        #region const

        #region action

        /// <summary>
        /// Action key for bCore Connection Changed
        /// </summary>
        public const string ActionKeyConnectionChanged = "LibBcore.BcoreStatusReceiver.ConnectionChanged";

        /// <summary>
        /// Action key for bCore Service Discovered
        /// </summary>
        public const string ActionKeyServiceDiscovered = "LibBcore.BcoreStatusReceiver.ServiceDiscovered";

        /// <summary>
        /// Action key for Read bCore Battery Voltage
        /// </summary>
        public const string ActionKeyReadBattery = "LibBcore.BcoreStatusReceiver.ReadBattery";

        /// <summary>
        /// Action key for Read bCore Function info
        /// </summary>
        public const string ActionKeyReadFunctions = "LibBcore.BcoreStatusReceiver.ReadFunction";

        #endregion

        #region extra

        /// <summary>
        /// Extra key for bCore MAC Address
        /// </summary>
        public const string ExtraKeyAddress = "LibBcore.BcoreStatusReceiver.ExtraAddress";

        /// <summary>
        /// Extra key for bCore Connection Status
        /// </summary>
        public const string ExtraKeyConnectionStatus = "LibBcore.BcoreStatusReceiver.ExtraConnectionStatus";

        /// <summary>
        /// Extra key for bCore Service discovered status
        /// </summary>
        public const string ExtraKeyIsDiscoveredService = "LibBcore.BcoreStatusReceiver.ExtraIsDiscoveredService";

        /// <summary>
        /// Extra key for bCore Battery Voltage
        /// </summary>
        public const string ExtraKeyBatteryVoltage = "LibBcore.BcoreStatusReceiver.ExtraBatteryVoltage";

        /// <summary>
        /// Extra key for bCore function info
        /// </summary>
        public const string ExtraKeyFunctions = "LibBcore.BcoreStatusReceiver.ExtraFunction";

        #endregion

        #endregion

        #region static

        /// <summary>
        /// Create IntentFilter for BcoreStatusReceiver
        /// </summary>
        /// <returns></returns>
        public static IntentFilter CreateFilter()
        {
            var filter = new IntentFilter();
            filter.AddAction(ActionKeyConnectionChanged);
            filter.AddAction(ActionKeyServiceDiscovered);
            filter.AddAction(ActionKeyReadBattery);
            filter.AddAction(ActionKeyReadFunctions);

            return filter;
        }

        #endregion

        #region event

        /// <summary>
        /// Change bCore connection status event
        /// </summary>
        public event EventHandler<BcoreConnectionChangedEventArgs> ChangedConnectionStatus;

        /// <summary>
        /// Discover bCore service event
        /// </summary>
        public event EventHandler<BcoreDiscoverdServiceEventArgs> DiscoveredService;

        /// <summary>
        /// Read bCore battery voltage event
        /// </summary>
        public event EventHandler<BcoreReadBatteryVoltageEventArgs> ReadBattery;

        /// <summary>
        /// Read bCore function event
        /// </summary>
        public event EventHandler<BcoreReadFunctionsEventArgs> ReadFunctions;

        #endregion

        public override void OnReceive(Context context, Intent intent)
        {
            switch (intent.Action)
            {
                case ActionKeyConnectionChanged:
                    OnChangedConnection(intent);
                    break;
                case ActionKeyServiceDiscovered:
                    OnDiscoveredService(intent);
                    break;
                case ActionKeyReadBattery:
                    OnReadBattery(intent);
                    break;
                case ActionKeyReadFunctions:
                    OnReadFunction(intent);
                    break;

            }
        }

        private void OnChangedConnection(Intent intent)
        {
            var address = intent.GetAddress();

            var state = intent.GetConnectionState();

            ChangedConnectionStatus?.Invoke(this, new BcoreConnectionChangedEventArgs(address, state));
        }

        private void OnDiscoveredService(Intent intent)
        {
            var address = intent.GetAddress();

            var result = intent.GetDiscoveredServiceResult();

            DiscoveredService?.Invoke(this, new BcoreDiscoverdServiceEventArgs(address, result));
        }

        private void OnReadBattery(Intent intent)
        {
            var address = intent.GetAddress();

            var voltage = intent.GetBatteryVoltage();

            ReadBattery?.Invoke(this, new BcoreReadBatteryVoltageEventArgs(address, voltage));
        }

        private void OnReadFunction(Intent intent)
        {
            var address = intent.GetAddress();

            var info = intent.GetFunctionInfo();

            ReadFunctions?.Invoke(this, new BcoreReadFunctionsEventArgs(address, info));
        }
    }
}