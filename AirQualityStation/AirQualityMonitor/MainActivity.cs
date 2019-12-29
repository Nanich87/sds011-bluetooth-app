namespace AirQualityMonitor
{
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
        private ListView listDevices;

        private ArrayAdapter<BluetoothDevice> deviceAdapter;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            int id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
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
                bluetoothManager.Adapter.Enable();

                deviceAdapter = new ArrayAdapter<BluetoothDevice>(this, Android.Resource.Layout.SimpleListItem1, bluetoothManager.Adapter.BondedDevices.ToList());
                listDevices.Adapter = deviceAdapter;
            }
        }

        private void OnDeviceClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (deviceAdapter.GetItem(e.Position) is BluetoothDevice device)
            {
                var intent = new Intent(this, typeof(BluetoothService));
                intent.SetAction(AirQualityMonitor.BluetoothService.ActionStartService);
                intent.PutExtra(AirQualityMonitor.BluetoothService.KeyDevice, device);

                if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                {
                    StartService(intent);
                }
                else
                {
                    StartForegroundService(intent);
                }
            }
        }
    }
}