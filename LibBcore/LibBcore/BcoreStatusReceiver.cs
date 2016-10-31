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

        public const string ActionKeyConnectionChanged = "LibBcore.BcoreStatusReceiver.ConnectionChanged";

        public const string ActionKeyServiceDiscovered = "LibBcore.BcoreStatusReceiver.ServiceDiscovered";

        public const string ActionKeyReadBattery = "LibBcore.BcoreStatusReceiver.ReadBattery";

        public const string ActionKeyReadFunctions = "LibBcore.BcoreStatusReceiver.ReadFunction";

        #endregion

        #region extra

        public const string ExtraKeyAddress = "LibBcore.BcoreStatusReceiver.ExtraAddress";

        public const string ExtraKeyConnectionStatus = "LibBcore.BcoreStatusReceiver.ExtraConnectionStatus";

        public const string ExtraKeyIsDiscoveredService = "LibBcore.BcoreStatusReceiver.ExtraIsDiscoveredService";

        public const string ExtraKeyBatteryVoltage = "LibBcore.BcoreStatusReceiver.ExtraBatteryVoltage";

        public const string ExtraKeyFunctions = "LibBcore.BcoreStatusReceiver.ExtraFunction";

        #endregion

        #endregion

        #region static

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

        public event EventHandler<BcoreConnectionChangedEventArgs> ChangedConnectionStatus;

        public event EventHandler<BcoreDiscoverdServiceEventArgs> DiscoveredService;

        public event EventHandler<BcoreReadBatteryVoltageEventArgs> ReadBattery;

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