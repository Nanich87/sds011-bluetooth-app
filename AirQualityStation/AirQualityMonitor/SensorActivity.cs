﻿namespace AirQualityMonitor
{
    using Android.App;
    using Android.Content;
    using Android.OS;
    using Android.Support.V7.App;
    using Android.Widget;

    [Activity(
        Label = "@string/sensor_title")]
    public class SensorActivity : AppCompatActivity
    {
        private BroadcastReceiver sensorReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_sensor);

            sensorReceiver = new SensorReceiver(this);
        }

        protected override void OnStart()
        {
            base.OnStart();

            RegisterReceiver(sensorReceiver, new IntentFilter(AirQualityMonitor.BluetoothService.ActionDataReceived));
        }

        protected override void OnStop()
        {
            UnregisterReceiver(sensorReceiver);

            base.OnStop();
        }

        private class SensorReceiver : BroadcastReceiver
        {
            private readonly TextView textPM25;
            private readonly TextView textPM10;
            private readonly TextView textTemperature;
            private readonly TextView textHumidity;

            public SensorReceiver(SensorActivity activity)
            {
                textPM25 = activity.FindViewById<TextView>(Resource.Id.text_pm25);
                textPM10 = activity.FindViewById<TextView>(Resource.Id.text_pm10);
                textTemperature = activity.FindViewById<TextView>(Resource.Id.text_temperature);
                textHumidity = activity.FindViewById<TextView>(Resource.Id.text_humidity);
            }

            public override void OnReceive(Context context, Intent intent)
            {
                textPM25.Post(() => textPM25.Text = string.Format("{0:F1}", intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyPM25, 0.0)));
                textPM10.Post(() => textPM10.Text = string.Format("{0:F1}", intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyPM10, 0.0)));
                textTemperature.Post(() => textTemperature.Text = string.Format("{0:F1}", intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyTemperature, 0.0)));
                textHumidity.Post(() => textHumidity.Text = string.Format("{0:F0}", intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyHumidity, 0.0)));
            }
        }
    }
}