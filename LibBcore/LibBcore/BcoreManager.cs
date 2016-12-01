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

namespace LibBcore
{
    public class BcoreManager : BluetoothGattCallback
    {
        #region field

        private readonly string _address;

        private readonly Context _context;

        private readonly BluetoothManager _bluetoothManager;

        private BluetoothGatt _gatt;

        private EBcoreConnectionState _connectionState = EBcoreConnectionState.Disconnected;

        private BluetoothGattService _service;

        private BluetoothGattCharacteristic _batteryCharacteristic;

        private BluetoothGattCharacteristic _motorPwmCharacteristic;

        private BluetoothGattCharacteristic _servoPosCharacteristic;

        private BluetoothGattCharacteristic _portOutCharacteristic;

        private BluetoothGattCharacteristic _functionCharacteristic;

        private byte[] _portOutState = {0};

        #endregion

        #region property

        private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

        private BluetoothAdapter BluetoothAdapter => _bluetoothManager?.Adapter;

        private byte PortOutState
        {
            get { return _portOutState[0]; }
            set { _portOutState[0] = value; }
        }

        #endregion

        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">Context.</param>
        /// <param name="address">Address of bCore to connect.</param>
        public BcoreManager(Context context, string address)
        {
            _context = context;
            _bluetoothManager = _context.GetSystemService(Context.BluetoothService) as BluetoothManager;

            _address = address;
        }

        ~BcoreManager()
        {
            Dispose();
        }

        #endregion

        #region method

        #region overrid BluetoothGattCallback

        public override void OnConnectionStateChange(BluetoothGatt gatt, GattStatus status, ProfileState newState)
        {
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
                    break;
            }

            SendBroadcastConnectionState();
        }

        public override void OnServicesDiscovered(BluetoothGatt gatt, GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);

            if (!IsConnectedDevice(gatt)) return;

            _service = gatt.GetService(BcoreUuid.BcoreService);

            _motorPwmCharacteristic = _service?.GetCharacteristic(BcoreUuid.MotorPwm);
            _servoPosCharacteristic = _service?.GetCharacteristic(BcoreUuid.ServoPos);
            _portOutCharacteristic = _service?.GetCharacteristic(BcoreUuid.PortOut);
            _batteryCharacteristic = _service?.GetCharacteristic(BcoreUuid.BatteryVol);
            _functionCharacteristic = _service?.GetCharacteristic(BcoreUuid.GetFunctions);

            PortOutState = 0;

            SendBroadcastDiscoveredService(_service != null);
        }

        public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);

            if (!IsConnectedDevice(gatt)) return;

            if (characteristic.Uuid == BcoreUuid.GetFunctions)
            {
                var value = characteristic.GetValue();

                SendBroadcastReadFunctions(value);
            }
            else if (characteristic.Uuid == BcoreUuid.BatteryVol)
            {
                var value = characteristic.GetBatteryVoltage();

                SendBroadcastReadBattery(value);
            }

            Semaphore.Release();
        }

        public override void OnCharacteristicWrite(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);

            if (!IsConnectedDevice(gatt)) return;

            Semaphore.Release();
        }

        #endregion

        #region public

        /// <summary>
        /// Dispose
        /// </summary>
        public new void Dispose()
        {
            base.Dispose();

            _gatt?.Close();

            _connectionState = EBcoreConnectionState.Disconnected;
            
            _gatt = null;
        }

        /// <summary>
        /// Connect to bCore
        /// </summary>
        /// <param name="isAuto">auto connect</param>
        public void Connect(bool isAuto = false)
        {
            if (_gatt != null)
            {
                if (_connectionState == EBcoreConnectionState.Connected || _connectionState == EBcoreConnectionState.Connecting) return;

                if (_gatt.Connect())
                {
                    _connectionState = EBcoreConnectionState.Connecting;
                    return;
                }

                _gatt.Close();
            }

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
        public async void WriteMotorPwm(int idx, int pwm, bool isFlip = false)
        {
            var value = Bcore.MakeMotorPwmValue(idx, pwm, isFlip);

            await WriteCharacteristic(_motorPwmCharacteristic, value);
        }

        /// <summary>
        /// Write servo position value
        /// </summary>
        /// <param name="idx">Servo index</param>
        /// <param name="pos">Servo posision</param>
        /// <param name="isFlip">true=Flip/false=not Flip</param>
        public async void WriteServoPos(int idx, int pos, bool isFlip = false)
        {
            var value = Bcore.MakeServoPosValue(idx, pos, isFlip);

            await WriteCharacteristic(_servoPosCharacteristic, value);
        }

        /// <summary>
        /// Write servo position value
        /// </summary>
        /// <param name="idx">Servo index</param>
        /// <param name="pos">Servo position</param>
        /// <param name="trim">Servo trim value</param>
        /// <param name="isFlip">true=Flip/false=not Flip</param>
        public async void WriteServoPos(int idx, int pos, int trim, bool isFlip = false)
        {
            var value = Bcore.MakeServoPosValue(idx, pos, trim, isFlip);

            await WriteCharacteristic(_servoPosCharacteristic, value);
        }

        /// <summary>
        /// Write port out value
        /// </summary>
        /// <param name="idx">Port out index</param>
        /// <param name="isOn">true=On/false=Off</param>
        public async void WritePortOut(int idx, bool isOn)
        {
            if (isOn)
            {
                PortOutState = (byte) (PortOutState & (0x01 << idx));
            }
            else
            {
                PortOutState = (byte) (PortOutState | ~(0x01 << idx));
            }

            await WriteCharacteristic(_portOutCharacteristic, _portOutState);
        }

        /// <summary>
        /// Write port out value
        /// </summary>
        /// <param name="state">Port out value set bit.</param>
        public async void WritePortOut(byte state)
        {
            PortOutState = state;

            await WriteCharacteristic(_portOutCharacteristic, _portOutState);
        }

        /// <summary>
        /// Read battery voltage
        /// </summary>
        public async void ReadBatteryVoltage()
        {
            await ReadCharacteristic(_batteryCharacteristic);
        }

        /// <summary>
        /// Read funtion infomation
        /// </summary>
        public async void ReadFunctions()
        {
            await ReadCharacteristic(_functionCharacteristic);
        }

        #endregion

        #region private

        private async Task WriteCharacteristic(BluetoothGattCharacteristic characteristic, byte[] value)
        {
            if (!characteristic.Properties.HasFlag(GattProperty.Write) &&
                !characteristic.Properties.HasFlag(GattProperty.WriteNoResponse))
                return;

            await Semaphore.WaitAsync();

            characteristic.SetValue(value);

            _gatt.WriteCharacteristic(characteristic);
        }

        private async Task ReadCharacteristic(BluetoothGattCharacteristic characteristic)
        {
            if (!characteristic.Properties.HasFlag(GattProperty.Read)) return;

            await Semaphore.WaitAsync();

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