namespace AirQualityMonitor
{
    using Android.Content;
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Views;

    public abstract class BaseActivity : AppCompatActivity
    {
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    {
                        OnBackPressed();

                        return true;
                    }

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


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
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