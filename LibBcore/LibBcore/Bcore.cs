using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.Bluetooth;
using Android.Media;
using Android.Text;

namespace LibBcore
{
    public enum EBcoreConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Disconnecting,
    }

    public static class Bcore
    {
        /// <summary>
        /// Max count of function
        /// </summary>
        public const int MaxFunctionCount = 4;

        /// <summary>
        /// Minimum value of motor pwm
        /// </summary>
        public const int MinMotorPwm = 0;

        /// <summary>
        /// Maximum value of motor pwm
        /// </summary>
        public const int MaxMotorPwm = 0xff;

        /// <summary>
        /// Stop value of motor pwm
        /// </summary>
        public const int StopMotorPwm = 0x80;

        /// <summary>
        /// Minimum value of servo position
        /// </summary>
        public const int MinServoPos = 0;

        /// <summary>
        /// Maximum value of servo position
        /// </summary>
        public const int MaxServoPos = 0xff;

        /// <summary>
        /// Center value of servo position
        /// </summary>
        public const int CenterServoPos = 0x80;

        private const int IdxBatteryVoltageLow = 0;
        private const int IdxBatteryVoltageHigh = 1;
        private const int BatteryVoltageDataLength = 2;

        internal static int GetBatteryVoltage(this BluetoothGattCharacteristic characteristic)
        {
            if (characteristic == null) return -1;
            if (characteristic.Uuid != BcoreUuid.BatteryVol) return -1;

            var value = characteristic.GetValue();

            if (value == null || value.Length != BatteryVoltageDataLength) return -1;

            return (value[IdxBatteryVoltageLow] & 0xff) | (value[IdxBatteryVoltageHigh] << 8);
        }

        internal static byte[] MakeMotorPwmValue(int idx, int pwm, bool isFlip = false)
        {
            if (idx < 0 || MaxFunctionCount <= idx) return null;

            if (isFlip)
            {
                pwm = MaxMotorPwm - pwm;
            }

            if (pwm < MinMotorPwm) pwm = MinMotorPwm;
            else if (pwm > MaxMotorPwm) pwm = MaxMotorPwm;

            return new[] {(byte) idx, (byte) pwm};
        }

        internal static byte[] MakeServoPosValue(int idx, int pos, bool isFlip = false)
        {
            if (idx < 0 || MaxFunctionCount <= idx) return null;

            if (isFlip)
            {
                pos = MaxServoPos - pos;
            }

            if (pos < MinServoPos) pos = MinServoPos;
            else if (pos > MaxServoPos) pos = MaxMotorPwm;

            return new[] { (byte)idx, (byte)pos };
        }

        internal static byte[] MakeServoPosValue(int idx, int pos, int trim, bool isFlip = false)
        {
            pos += trim;

            return MakeServoPosValue(idx, pos, isFlip);
        }
    }
}
