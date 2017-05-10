using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using Java.Util;
using Javax.Crypto.Interfaces;

namespace LibBcore
{
    public class BcoreManager : BluetoothGattCallback
    {
        #region const

        private static readonly string TAG = typeof(BcoreManager).Name;

        #endregion

        #region field

        private string _address;

        private readonly Context _context;

        private BluetoothManager BluetoothManager => _context?.GetSystemService(Context.BluetoothService) as BluetoothManager;

        private BluetoothGatt _gatt;

        private EBcoreConnectionState _connectionState = EBcoreConnectionState.Disconnected;

        private BluetoothGattService _service;

        private BluetoothGattCharacteristic _batteryCharacteristic;

        private BluetoothGattCharacteristic _motorPwmCharacteristic;

        private BluetoothGattCharacteristic _servoPosCharacteristic;

        private BluetoothGattCharacteristic _portOutCharacteristic;

        private BluetoothGattCharacteristic _burstCmdCharacteristic;

        private BluetoothGattCharacteristic _functionCharacteristic;

        private byte[] _portOutState = {0};

        #endregion

        #region property

        private BluetoothAdapter BluetoothAdapter => BluetoothManager?.Adapter;

        private byte PortOutState
        {
            get { return _portOutState[0]; }
            set { _portOutState[0] = value; }
        }

        public bool IsEnableBurst => _burstCmdCharacteristic != null;

        public bool IsConnected => _connectionState == EBcoreConnectionState.Connected;

        public bool IsConnecting => _connectionState == EBcoreConnectionState.Connecting;

        #endregion

        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="address">Address of bCore to connect.</param>
        public BcoreManager(Context context)
        {
            _context = context;
        }

        #endregion

        #region method

        #region overrid BluetoothGattCallback

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
            Android.Util.Log.Info(TAG, $"bCore connection changed:{newState}");
            base.OnConnectionStateChange(gatt, status, newState);

            if (!IsConnectedDevice(gatt)) return;

            switch (newState)
            {
                case ProfileState.Connected:
                    gatt.DiscoverServices();
                    _connectionState = EBcoreConnectionState.Connected;
                    break;
                case ProfileState.Connecting:
                    _connectionState = EBcoreConnectionState.Connecting;
                    break;
                case ProfileState.Disconnecting:
                    _connectionState = EBcoreConnectionState.Disconnecting;
                    break;
                case ProfileState.Disconnected:
                    _connectionState = EBcoreConnectionState.Disconnected;
                    gatt.Close();
                    _gatt = null;
                    break;
            }

            SendBroadcastConnectionState();
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            Android.Util.Log.Info(TAG, $"bCore service discovered:{status}");
            base.OnServicesDiscovered(gatt, status);

            if (!IsConnectedDevice(gatt)) return;

            _service = gatt.GetService(BcoreUuid.BcoreService);
            Android.Util.Log.Debug("bCore Manager", $"char count:{_service.Characteristics.Count}");
            _motorPwmCharacteristic = _service?.GetCharacteristic(BcoreUuid.MotorPwm);
            _servoPosCharacteristic = _service?.GetCharacteristic(BcoreUuid.ServoPos);
            _portOutCharacteristic = _service?.GetCharacteristic(BcoreUuid.PortOut);
            _batteryCharacteristic = _service?.GetCharacteristic(BcoreUuid.BatteryVol);
            _functionCharacteristic = _service?.GetCharacteristic(BcoreUuid.GetFunctions);
            _burstCmdCharacteristic = _service?.GetCharacteristic(BcoreUuid.BurstCmd);

            PortOutState = 0;

            SendBroadcastDiscoveredService(_service != null);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            Android.Util.Log.Debug("bCore Manager", $"on read characteristic");

            if (!IsConnectedDevice(gatt)) return;

            if (characteristic.Uuid.Equals(BcoreUuid.GetFunctions))
            {
                var value = characteristic.GetValue();
                Android.Util.Log.Debug("bCore Manager", $"read function:{value}");

                SendBroadcastReadFunctions(value);
            }
            else if (characteristic.Uuid.Equals(BcoreUuid.BatteryVol))
            {
                var value = characteristic.GetBatteryVoltage();
                Android.Util.Log.Debug("bCore Manager", $"read voltate:{value}");

                SendBroadcastReadBattery(value);
            }
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            Android.Util.Log.Info(TAG, $"bCore write characteristic:{characteristic.Uuid}/{status}");
            base.OnCharacteristicWrite(gatt, characteristic, status);

            if (!IsConnectedDevice(gatt)) return;
        }

        #endregion

        #region public

        /// <summary>
        /// Connect to bCore
        /// </summary>
        /// <param name="address"></param>
        /// <param name="isAuto">auto connect</param>
        public void Connect(string address, bool isAuto = false)
        {
            _address = address;

            var device = BluetoothAdapter.GetRemoteDevice(_address);

            if (device == null) return;

            _gatt = device.ConnectGatt(_context, isAuto, this);

            _connectionState = EBcoreConnectionState.Connecting;
        }

        /// <summary>
        /// Disconnet bCore
        /// </summary>
        public void Disconnect()
        {
            if (_gatt == null || _connectionState == EBcoreConnectionState.Disconnecting || _connectionState == EBcoreConnectionState.Disconnected)
                return;

            _gatt.Disconnect();
            _connectionState = EBcoreConnectionState.Disconnecting;
        }

        /// <summary>
        /// Write motor pwm value
        /// </summary>
        /// <param name="idx">Motor index</param>
        /// <param name="pwm">Motor PWM value</param>
        /// <param name="isFlip">true=Flip/false=not Flip</param>
        public void WriteMotorPwm(int idx, int pwm, bool isFlip = false)
        {
            var value = Bcore.MakeMotorPwmValue(idx, pwm, isFlip);

            WriteCharacteristic(_motorPwmCharacteristic, value);
        }

        /// <summary>
        /// Write servo position value
        /// </summary>
        /// <param name="idx">Servo index</param>
        /// <param name="pos">Servo posision</param>
        /// <param name="isFlip">true=Flip/false=not Flip</param>
        public void WriteServoPos(int idx, int pos, bool isFlip = false)
        {
            var value = Bcore.MakeServoPosValue(idx, pos, isFlip);

            WriteCharacteristic(_servoPosCharacteristic, value);
        }

        /// <summary>
        /// Write servo position value
        /// </summary>
        /// <param name="idx">Servo index</param>
        /// <param name="pos">Servo position</param>
        /// <param name="trim">Servo trim value</param>
        /// <param name="isFlip">true=Flip/false=not Flip</param>
        public void WriteServoPos(int idx, int pos, int trim, bool isFlip = false)
        {
            var value = Bcore.MakeServoPosValue(idx, pos, trim, isFlip);

            WriteCharacteristic(_servoPosCharacteristic, value);
        }

        /// <summary>
        /// Write port out value
        /// </summary>
        /// <param name="idx">Port out index</param>
        /// <param name="isOn">true=On/false=Off</param>
        public void WritePortOut(int idx, bool isOn)
        {
            if (isOn)
            {
                PortOutState = (byte) (PortOutState | (0x01 << idx));
            }
            else
            {
                PortOutState = (byte) (PortOutState & ~(0x01 << idx));
            }

            WriteCharacteristic(_portOutCharacteristic, _portOutState);
        }

        /// <summary>
        /// Write port out value
        /// </summary>
        /// <param name="state">Port out value set bit.</param>
        public void WritePortOut(byte state)
        {
            PortOutState = state;

            WriteCharacteristic(_portOutCharacteristic, _portOutState);
        }

        /// <summary>
        /// Write burst command
        /// </summary>
        /// <param name="data">data array(mmpsss:7bytes)</param>
        public void WriteBurstCommand(byte[] data)
        {
            if (!IsEnableBurst) return;

            if (data.Length != 7) return;

            PortOutState = data[6];

            WriteCharacteristic(_burstCmdCharacteristic, data);
        }

        /// <summary>
        /// Write busrt command
        /// </summary>
        /// <param name="mtr">motor pwm array[2]</param>
        /// <param name="svr">servo pwm array[4]</param>
        /// <param name="portOut">port out status</param>
        public void WriteBurstCommand(int[] mtr, int[] svr, byte portOut)
        {
            if (!IsEnableBurst) return;

            var value = Bcore.MakeBurstCommandValue(mtr, svr, portOut);

            PortOutState = portOut;

            WriteCharacteristic(_burstCmdCharacteristic, value);
        }

        /// <summary>
        /// Write burst command
        /// </summary>
        /// <param name="mtr0">motor ch0 pwm</param>
        /// <param name="mtr1">motor ch1 pwm</param>
        /// <param name="svr0">servo ch0 pos</param>
        /// <param name="svr1">servo ch1 pos</param>
        /// <param name="svr2">servo ch2 pos</param>
        /// <param name="svr3">servo ch3 pos</param>
        /// <param name="portOut">port out status</param>
        public void WriteBurstCommand(int mtr0, int mtr1, int svr0, int svr1, int svr2, int svr3, byte portOut)
        {
            if (!IsEnableBurst) return;

            var value = Bcore.MakeBurstCommandValue(mtr0, mtr1, svr0, svr1, svr2, svr3, portOut);

            PortOutState = portOut;

            WriteCharacteristic(_burstCmdCharacteristic, value);
        }

        /// <summary>
        /// Read battery voltage
        /// </summary>
        public void ReadBatteryVoltage()
        {
            Android.Util.Log.Debug("bCore Manager", $"start read battery voltage");
            ReadCharacteristic(_batteryCharacteristic);
        }

        /// <summary>
        /// Read funtion infomation
        /// </summary>
        public void ReadFunctions()
        {
            ReadCharacteristic(_functionCharacteristic);
        }

        #endregion

        #region private

        private void WriteCharacteristic(BluetoothGattCharacteristic characteristic, byte[] value)
        {
            if (characteristic == null ||
                (!characteristic.Properties.HasFlag(GattProperty.Write) &&
                !characteristic.Properties.HasFlag(GattProperty.WriteNoResponse)))
                return;

            characteristic.SetValue(value);

            _gatt.WriteCharacteristic(characteristic);
        }

        private void ReadCharacteristic(BluetoothGattCharacteristic characteristic)
        {
            if (!characteristic.Properties.HasFlag(GattProperty.Read)) return;

            _gatt.ReadCharacteristic(characteristic);
        }

        private void SendBcoreBroadcast(Intent intent)
        {
            intent.PutExtra(BcoreStatusReceiver.ExtraKeyAddress, _address);

            _context.SendBroadcast(intent);
        }

        private void SendBroadcastConnectionState()
        {
            var intent = new Intent(BcoreStatusReceiver.ActionKeyConnectionChanged);

            intent.PutExtra(BcoreStatusReceiver.ExtraKeyConnectionStatus, (int) _connectionState);

            SendBcoreBroadcast(intent);
        }

        private void SendBroadcastDiscoveredService(bool isDiscovered)
        {
            var intent = new Intent(BcoreStatusReceiver.ActionKeyServiceDiscovered);

            intent.PutExtra(BcoreStatusReceiver.ExtraKeyIsDiscoveredService, isDiscovered);

            SendBcoreBroadcast(intent);
        }

        private void SendBroadcastReadBattery(int voltage)
        {
            var intent = new Intent(BcoreStatusReceiver.ActionKeyReadBattery);

            intent.PutExtra(BcoreStatusReceiver.ExtraKeyBatteryVoltage, voltage);

            SendBcoreBroadcast(intent);
        }

        private void SendBroadcastReadFunctions(byte[] value)
        {
            var intent = new Intent(BcoreStatusReceiver.ActionKeyReadFunctions);

            intent.PutExtra(BcoreStatusReceiver.ExtraKeyFunctions, value);

            SendBcoreBroadcast(intent);
        }

        private bool IsConnectedDevice(BluetoothGatt gatt)
        {
            if (gatt?.Device?.Address == null) return false;

            return string.Compare(_address, gatt.Device.Address, StringComparison.OrdinalIgnoreCase) == 0;
        }

        #endregion

        #endregion
    }
}