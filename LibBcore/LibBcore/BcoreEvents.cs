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
    /// bCore base event arguments
    /// </summary>
    public abstract class BcoreEventArgs : EventArgs
    {
        /// <summary>
        /// Device address of bCore
        /// </summary>
        public string Address { get; }

        protected BcoreEventArgs(string address)
        {
            Address = address;
        }
    }

    /// <summary>
    /// bCore connection status changed event arguments
    /// </summary>
    public class BcoreConnectionChangedEventArgs : BcoreEventArgs
    {
        /// <summary>
        /// New connection status
        /// </summary>
        public EBcoreConnectionState State { get; }

        public BcoreConnectionChangedEventArgs(string address, EBcoreConnectionState state) : base(address)
        {
            State = state;
        }
    }

    /// <summary>
    /// bCore discovered service event argumetns
    /// </summary>
    public class BcoreDiscoverdServiceEventArgs : BcoreEventArgs
    {
        /// <summary>
        /// Discovered service result.
        /// true=Discovered bCore GATT Service/false=Not discovered bCore GATT Service
        /// </summary>
        public bool IsDiscoveredService { get; }

        public BcoreDiscoverdServiceEventArgs(string address, bool isDiscoveredService) : base(address)
        {
            IsDiscoveredService = isDiscoveredService;
        }
    }

    /// <summary>
    /// bCore read battery voltage event arguments
    /// </summary>
    public class BcoreReadBatteryVoltageEventArgs : BcoreEventArgs
    {
        /// <summary>
        /// Battery voltage[mV]
        /// </summary>
        public int Value { get; }

        public BcoreReadBatteryVoltageEventArgs(string address, int value) : base(address)
        {
            Value = value;
        }
    }

    /// <summary>
    /// bCore read function event arguments
    /// </summary>
    public class BcoreReadFunctionsEventArgs : BcoreEventArgs
    {
        /// <summary>
        /// bCore function infomation
        /// </summary>
        public BcoreFunctionInfo FunctionInfo { get; }

        public BcoreReadFunctionsEventArgs(string address, BcoreFunctionInfo functionInfo) : base(address)
        {
            FunctionInfo = functionInfo;
        }
    }
}