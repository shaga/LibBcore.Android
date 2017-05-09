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
    /// <summary>
    /// bCore funtion information
    /// </summary>
    public class BcoreFunctionInfo
    {
        private const int IdxMotor = 0;
        private const int IdxServoPortOut = 1;
        private const int FunctionInfoLength = 2;
        private const int OffsetMotor = 0;
        private const int OffsetServo = 0;
        private const int OffsetPortOut = 4;

        private readonly byte[] _functionInfo;

        public byte[] FunctionInfo => _functionInfo;

        /// <summary>
        /// Enabled motor port count
        /// </summary>
        public int EnableMotorCount { get; }

        /// <summary>
        /// Enabled servo port count
        /// </summary>
        public int EnableServoCount { get; }

        /// <summary>
        /// Enabled port out count
        /// </summary>
        public int EnablePortOutCount { get; }

        public BcoreFunctionInfo(byte[] functionInfo)
        {
            EnableMotorCount = 0;
            EnableServoCount = 0;
            EnablePortOutCount = 0;

            if (functionInfo == null || functionInfo.Length != FunctionInfoLength) return;

            _functionInfo = functionInfo;

            for (var i = 0; i < Bcore.MaxFunctionCount; i++)
            {
                if (IsEnableMotor(i)) EnableMotorCount++;
                if (IsEnableServo(i)) EnableServoCount++;
                if (IsEnablePortOut(i)) EnablePortOutCount++;
            }
        }

        /// <summary>
        /// Check motor port is enable.
        /// </summary>
        /// <param name="idx">port index</param>
        /// <returns></returns>
        public bool IsEnableMotor(int idx)
        {
            return IsEnableFunction(idx, OffsetMotor, _functionInfo[IdxMotor]);
        }

        /// <summary>
        /// Check servo port is enable.
        /// </summary>
        /// <param name="idx">port index</param>
        /// <returns></returns>
        public bool IsEnableServo(int idx)
        {
            return IsEnableFunction(idx, OffsetServo, _functionInfo[IdxServoPortOut]);
        }

        /// <summary>
        /// Check port out is enable.
        /// </summary>
        /// <param name="idx">port index</param>
        /// <returns></returns>
        public bool IsEnablePortOut(int idx)
        {
            return IsEnableFunction(idx, OffsetPortOut, _functionInfo[IdxServoPortOut]);
        }

        private bool IsEnableFunction(int idx, int offset, int value)
        {
            if (_functionInfo == null || idx < 0 || Bcore.MaxFunctionCount <= idx) return false;
            return ((value >> (idx + offset)) & 0x01) == 0x01;
        }
    }
}