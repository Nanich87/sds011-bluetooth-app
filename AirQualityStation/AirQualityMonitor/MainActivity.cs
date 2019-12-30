namespace AirQualityMonitor
{
    using System.Collections.Generic;
    using System.Linq;
    using Android.App;
    using Android.Bluetooth;
    using Android.Content;
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Views;
    using Android.Widget;

    [Activity(
        Label = "@string/app_name",
        MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int RequestEnableBluetooth = 1;


        private ListView listDevices;

        private ArrayAdapter<BluetoothDevice> deviceAdapter;


        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_stop_service:
                    {
                        StopBluetoothService();

                        return true;
                    }

                default:
                    {
                        return base.OnOptionsItemSelected(item);
                    }
            }
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            switch (requestCode)
            {
                case RequestEnableBluetooth:
                    {
                        if (resultCode == Result.Ok && GetSystemService(BluetoothService) is BluetoothManager bluetoothManager)
                        {
                            PopulateDeviceList(bluetoothManager.Adapter.BondedDevices);
                        }

                        break;
                    }

                default:
                    {
                        base.OnActivityResult(requestCode, resultCode, data);

                        break;
                    }
            }
        }


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            listDevices = FindViewById<ListView>(Resource.Id.list_device);
            if (listDevices != null)
            {
                listDevices.ItemClick += OnDeviceClick;
            }
        }

        protected override void OnStart()
        {
            base.OnStart();

            if (GetSystemService(BluetoothService) is BluetoothManager bluetoothManager)
            {
                if (!bluetoothManager.Adapter.IsEnabled)
                {
                    StartActivityForResult(new Intent(BluetoothAdapter.ActionRequestEnable), RequestEnableBluetooth);
                }
                else
                {
                    PopulateDeviceList(bluetoothManager.Adapter.BondedDevices);
                }
            }
        }


        private void OnDeviceClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (deviceAdapter.GetItem(e.Position) is BluetoothDevice device)
            {
                StartBluetoothService(device);
            }
        }


        private void PopulateDeviceList(ICollection<BluetoothDevice> devices)
        {
            if (devices != null)
            {
                deviceAdapter = new ArrayAdapter<BluetoothDevice>(this, Android.Resource.Layout.SimpleListItem1, devices.ToArray());
                listDevices.Adapter = deviceAdapter;
            }
        }

        private void StartBluetoothService(BluetoothDevice device)
        {
            var intent = new Intent(this, typeof(BluetoothService));
            intent.SetAction(AirQualityMonitor.BluetoothService.ActionStartService);
            intent.PutExtra(AirQualityMonitor.BluetoothService.KeyDevice, device);

            try
            {
                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    StartService(intent);
                }
                else
                {
                    StartForegroundService(intent);
                }
            }
            catch (Java.Lang.SecurityException)
            {
            }
        }

        private void StopBluetoothService()
        {
            var intent = new Intent(this, typeof(BluetoothService));
            intent.SetAction(AirQualityMonitor.BluetoothService.ActionStopService);

            try
            {
                StartService(intent);
            }
            catch (Java.Lang.SecurityException)
            {
            }
        }
    }
}