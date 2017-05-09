using  System;
using  System.Linq;
using  System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Widget;
using Android.OS;
using LibBcore;

namespace LibBcoreTest
{
    [Activity(Label = "LibBcoreTest", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private IList<BcoreDeviceInfo> _list;
        private BcoreAdapter _adapter;
        private ListView _listView;
        private Button _btnConnect;
        private Button _btnScan;
        private Button _btnBattery;
        private Button _btnFunctions;
        private Button _btnMotor;
        private Button _btnServo;
        private Button _btnPortOut;
        private EditText _editMotorCh;
        private EditText _editMotorValue;
        private EditText _editServoCh;
        private EditText _editServoValue;
        private EditText _editPortOutCh;
        private EditText _editPortOutValue;
        private EditText _editBattery;
        private EditText _editFunctions;

        private BcoreStatusReceiver _receiver;

        private BcoreDeviceInfo _selededInfo = null;

        private BcoreScanner _scanner;

        private bool _canScanning;

        private bool _isConnected;

        private BcoreManager _manager;

        private bool CanScanning
        {
            get { return _canScanning; }
            set
            {
                _canScanning = value;

                _btnScan.Enabled = _canScanning;
                IsConnected = false;
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;

                _btnBattery.Enabled = _isConnected;
                _btnFunctions.Enabled = _isConnected;
                _btnMotor.Enabled = _isConnected;
                _btnServo.Enabled = _isConnected;
                _editMotorCh.Enabled = _isConnected;
                _editMotorValue.Enabled = _isConnected;
                _editServoCh.Enabled = _isConnected;
                _editServoValue.Enabled = _isConnected;
                _btnPortOut.Enabled = _isConnected;
                _editPortOutCh.Enabled = _isConnected;
                _editPortOutValue.Enabled = _isConnected;
            }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            _btnBattery = FindViewById<Button>(Resource.Id.btn_read_battery);
            _editBattery = FindViewById<EditText>(Resource.Id.edit_battery);
            _btnBattery.Click += (sender, args) => _manager?.ReadBatteryVoltage();

            _btnFunctions = FindViewById<Button>(Resource.Id.btn_read_functions);
            _editFunctions = FindViewById<EditText>(Resource.Id.edit_functions);
            _btnFunctions.Click += (sender, args) => _manager?.ReadFunctions();

            _btnMotor = FindViewById<Button>(Resource.Id.btn_motor);
            _editMotorCh = FindViewById<EditText>(Resource.Id.edit_motor_ch);
            _editMotorValue = FindViewById<EditText>(Resource.Id.edit_motor_val);
            _btnMotor.Click += (sender, args) =>
            {
                var ch = int.Parse(_editMotorCh.Text, NumberStyles.AllowHexSpecifier);
                var value = int.Parse(_editMotorValue.Text, NumberStyles.AllowHexSpecifier);

                _manager?.WriteMotorPwm(ch & 0xff, value & 0xff);
            };

            _btnServo = FindViewById<Button>(Resource.Id.btn_servo);
            _editServoCh = FindViewById<EditText>(Resource.Id.edit_servo_ch);
            _editServoValue = FindViewById<EditText>(Resource.Id.edit_servo_val);
            _btnServo.Click += (sender, args) =>
            {
                var ch = int.Parse(_editServoCh.Text, NumberStyles.AllowHexSpecifier);
                var value = int.Parse(_editServoValue.Text, NumberStyles.AllowHexSpecifier);

                _manager?.WriteServoPos(ch, value);
            };

            _btnPortOut = FindViewById<Button>(Resource.Id.btn_po);
            _editPortOutCh = FindViewById<EditText>(Resource.Id.edit_po_ch);
            _editPortOutValue = FindViewById<EditText>(Resource.Id.edit_po_val);
            _btnPortOut.Click += (sender, args) =>
            {
                var ch = 0;
                if (!int.TryParse(_editPortOutCh.Text, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out ch))
                {
                    return;
                }

                var val = 0;
                if (!int.TryParse(_editPortOutValue.Text, out val)) return;

                _manager?.WritePortOut(ch, val > 0);
            };

            _list = new List<BcoreDeviceInfo>();
            _adapter = new BcoreAdapter(this, _list);

            _listView = FindViewById<ListView>(Resource.Id.list_found_bcore);
            _listView.ChoiceMode = ChoiceMode.Single;
            _listView.ItemClick += (sender, args) =>
            {
                _selededInfo = _adapter[args.Position];
                _btnConnect.Enabled = _selededInfo != null;
            };
            _listView.Adapter = _adapter;


            _receiver = new BcoreStatusReceiver();
            _receiver.ChangedConnectionStatus += OnChangedConnection;
            _receiver.DiscoveredService += OnDiscoveredService;
            _receiver.ReadBattery += OnReadBattery;
            _receiver.ReadFunctions += OnReadFunctions;

            _scanner = new BcoreScanner(this);
            _scanner.FoundBcore += (sender, info) =>
            {
                RunOnUiThread(() =>
                {
                    if (_list.Any(i => i.Address == info.Address)) return;
                    _list.Add(info);
                    _adapter.NotifyDataSetChanged();
                });
            };
            _btnScan = FindViewById<Button>(Resource.Id.btn_scan);
            _btnScan.Click += (sender, args) =>
            {
                if (_scanner.IsScanning)
                {
                    _scanner.StopScan();
                    _btnScan.Text = "SCAN START";
                    _listView.Enabled = true;
                }
                else
                {
                    _listView.Enabled = false;
                    for (var i = 0; i < _list.Count; i++)
                    {
                        _listView.SetItemChecked(i, false);
                    }
                    _selededInfo = null;
                    _btnConnect.Enabled = false;
                    _list.Clear();
                    _adapter.NotifyDataSetChanged();
                    _scanner.StartScan();
                    _btnScan.Text = "SCAN STOP";
                }
            };

            _btnConnect = FindViewById<Button>(Resource.Id.btn_connect);
            _btnConnect.Enabled = false;
            _btnConnect.Click += (sender, args) =>
            {
                if (IsConnected)
                {
                    _manager?.Disconnect();
                }
                else
                {
                    if (_selededInfo == null) return;

                    _manager?.Dispose();
                    _manager = new BcoreManager(this, _selededInfo.Address);
                    _manager.Connect();
                    _btnConnect.Enabled = false;
                    _listView.Enabled = false;
                }
            };
        }

        protected override void OnResume()
        {
            base.OnResume();

            RegisterReceiver(_receiver, BcoreStatusReceiver.CreateFilter());
        }

        protected override void OnPause()
        {
            base.OnPause();

            UnregisterReceiver(_receiver);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _receiver.ChangedConnectionStatus -= OnChangedConnection;
            _receiver.DiscoveredService -= OnDiscoveredService;
            _receiver.ReadBattery -= OnReadBattery;
            _receiver.ReadFunctions -= OnReadFunctions;
        }

        private void OnChangedConnection(object sender, BcoreConnectionChangedEventArgs e)
        {
            RunOnUiThread(() =>
            {
                switch (e.State)
                {
                    case EBcoreConnectionState.Connected:
                        CanScanning = false;
                        _btnConnect.Text = "Disconnect";
                        _btnConnect.Enabled = true;
                        break;
                    case EBcoreConnectionState.Disconnected:
                        _manager?.Dispose();
                        _manager = null;
                        _btnConnect.Enabled = _selededInfo != null;
                        _btnConnect.Text = "Connect";
                        _listView.Enabled = false;
                        IsConnected = false;
                        CanScanning = true;
                        break;

                }
            });
        }

        private void OnDiscoveredService(object sender, BcoreDiscoverdServiceEventArgs e)
        {
            if (e.IsDiscoveredService)
            {
                IsConnected = true;
                _manager?.ReadBatteryVoltage();
            }
            else
            {
                _manager?.Disconnect();
            }
        }

        private void OnReadBattery(object sender, BcoreReadBatteryVoltageEventArgs e)
        {
            RunOnUiThread(() =>
            {
                _editBattery.Text = e.Value.ToString();
            });
        }

        private void OnReadFunctions(object sender, BcoreReadFunctionsEventArgs e)
        {
            RunOnUiThread(() =>
            {
                _editFunctions.Text = string.Concat(e.FunctionInfo.FunctionInfo.Select(v => v.ToString("X2")));
            });
        }
    }
}
