// Copyright (c) GNNMobile.eu. All rights reserved.

namespace AirQualityMonitor
{
    using AirQualityMonitor.Extensions;
    using Android.App;
    using Android.Content;
    using Android.Content.Res;
    using Android.Graphics;
    using Android.OS;
    using Android.Widget;

    [Activity(
        Label = "@string/sensor_title")]
    public class SensorActivity : BaseActivity
    {
        private BroadcastReceiver sensorReceiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_sensor);

            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetDisplayShowHomeEnabled(true);

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

            private readonly TypedArray temperatureColors;
            private readonly TypedArray humidityColors;

            public SensorReceiver(SensorActivity activity)
            {
                textPM25 = activity.FindViewById<TextView>(Resource.Id.text_pm25);
                textPM10 = activity.FindViewById<TextView>(Resource.Id.text_pm10);
                textTemperature = activity.FindViewById<TextView>(Resource.Id.text_temperature);
                textHumidity = activity.FindViewById<TextView>(Resource.Id.text_humidity);

                temperatureColors = activity.Resources.ObtainTypedArray(Resource.Array.temperature_colors);
                humidityColors = activity.Resources.ObtainTypedArray(Resource.Array.humidity_colors);
            }

            public override void OnReceive(Context context, Intent intent)
            {
                double pm25 = intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyPM25, 0.0);
                int aqi25 = AQIExt.GetAQI25(pm25);
                Color aqi25Color = AQIExt.GetColor(aqi25);
                Color aqi25TextColor = AQIExt.GetTextColor(aqi25Color);

                textPM25.Text = string.Format("{0:F1}μg/m³{1}{2}", pm25, System.Environment.NewLine, aqi25);
                textPM25.SetBackgroundColor(aqi25Color);
                textPM25.SetTextColor(aqi25TextColor);

                double pm10 = intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyPM10, 0.0);
                int aqi10 = AQIExt.GetAQI25(pm10);
                Color aqi10Color = AQIExt.GetColor(aqi10);
                Color aqi10TextColor = AQIExt.GetTextColor(aqi10Color);

                textPM10.Text = string.Format("{0:F1}μg/m³{1}{2}", pm10, System.Environment.NewLine, aqi10);
                textPM10.SetBackgroundColor(aqi10Color);
                textPM10.SetTextColor(aqi10TextColor);

                double temperature = intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyTemperature, 0.0);
                int temperatureColorIndex = (int)temperature < 0 ? 0 : (int)temperature > 90 ? 90 : (int)temperature;

                textTemperature.Text = string.Format("{0:F1}°C", temperature);
                textTemperature.SetBackgroundColor(temperatureColors.GetColor(temperatureColorIndex, 0));

                double humidity = intent.GetDoubleExtra(AirQualityMonitor.BluetoothService.KeyHumidity, 0.0);
                int humidityColorIndex = (int)humidity < 0 ? 0 : (int)humidity > 100 ? 100 : (int)humidity;

                textHumidity.Text = string.Format("{0:F0}%", humidity);
                textHumidity.SetBackgroundColor(humidityColors.GetColor(humidityColorIndex, 0));
            }
        }
    }
}