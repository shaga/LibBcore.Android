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
            if (!characteristic.Uuid.Equals(BcoreUuid.BatteryVol)) return -1;

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

        internal static byte[] MakeBurstCommandValue(int[] mtr, int[] svr, byte portOut)
        {
            var value = new byte[7];

            var idx = 0;

            foreach (var m in mtr)
            {
                var v = m;

                if (v < MinMotorPwm) v = MinMotorPwm;
                else if (v > MaxMotorPwm) v = MaxMotorPwm;

                value[idx++] = (byte) v;

                if (idx >= 2) break;
            }

            value[idx++] = portOut;

            foreach (var s in svr)
            {
                var v = s;

                if (v < MinServoPos) v = MinServoPos;
                else if (v > MaxServoPos) v = MaxServoPos;

                value[idx++] = (byte) v;
                if (idx >= value.Length) break;
            }

            return value;
        }

        internal static byte[] MakeBurstCommandValue(int mtr0, int mtr1, int svr0, int svr1, int svr2, int svr3,
            byte portOut)
        {
            if (mtr0 < MinMotorPwm) mtr0 = MinMotorPwm;
            else if (mtr0 > MaxMotorPwm) mtr0 = MaxMotorPwm;

            if (mtr1 < MinMotorPwm) mtr1 = MinMotorPwm;
            else if (mtr1 > MaxMotorPwm) mtr1 = MaxMotorPwm;

            if (svr0 < MinServoPos) svr0 = MinServoPos;
            else if (svr0 > MaxServoPos) svr0 = MaxServoPos;

            if (svr1 < MinServoPos) svr1 = MinServoPos;
            else if (svr1 > MaxServoPos) svr1 = MaxServoPos;

            if (svr2 < MinServoPos) svr2 = MinServoPos;
            else if (svr2 > MaxServoPos) svr2 = MaxServoPos;

            if (svr3 < MinServoPos) svr3 = MinServoPos;
            else if (svr3 > MaxServoPos) svr3 = MaxServoPos;


            return new byte[] {(byte) mtr0, (byte) mtr1, (byte) svr0, (byte) svr1, (byte) svr2, (byte) svr3, portOut};
        }
    }
}
