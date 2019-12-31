// Copyright (c) GNNMobile.eu. All rights reserved.

namespace AirQualityMonitor.Extensions
{
    using Android.Graphics;

    internal static class AQIExt
    {
        public static Color GetColor(int aqi)
        {
            if (aqi >= 50 && aqi < 100)
            {
                return Color.Yellow;
            }
            else if (aqi >= 100 && aqi < 150)
            {
                return Color.Orange;
            }
            else if (aqi >= 150 && aqi < 200)
            {
                return Color.Red;
            }
            else if (aqi >= 200 && aqi < 300)
            {
                return Color.Purple;
            }
            else if (aqi >= 300)
            {
                return Color.Brown;
            }
            else
            {
                return Color.Lime;
            }
        }

        public static Color GetTextColor(Color color)
        {
            if (color == Color.Yellow || color == Color.Lime)
            {
                return Color.Black;
            }
            else
            {
                return Color.White;
            }
        }

        public static int GetAQI25(double pm25)
        {
            const double pm1 = 0;
            const double pm2 = 12;
            const double pm3 = 35.4;
            const double pm4 = 55.4;
            const double pm5 = 150.4;
            const double pm6 = 250.4;
            const double pm7 = 350.4;
            const double pm8 = 500.4;

            const int aqi1 = 0;
            const int aqi2 = 50;
            const int aqi3 = 100;
            const int aqi4 = 150;
            const int aqi5 = 200;
            const int aqi6 = 300;
            const int aqi7 = 400;
            const int aqi8 = 500;

            var aqiPM25 = 0.0;

            if (pm25 >= pm1 && pm25 <= pm2)
            {
                aqiPM25 = ((aqi2 - aqi1) / (pm2 - pm1) * (pm25 - pm1)) + aqi1;
            }
            else if (pm25 >= pm2 && pm25 <= pm3)
            {
                aqiPM25 = ((aqi3 - aqi2) / (pm3 - pm2) * (pm25 - pm2)) + aqi2;
            }
            else if (pm25 >= pm3 && pm25 <= pm4)
            {
                aqiPM25 = ((aqi4 - aqi3) / (pm4 - pm3) * (pm25 - pm3)) + aqi3;
            }
            else if (pm25 >= pm4 && pm25 <= pm5)
            {
                aqiPM25 = ((aqi5 - aqi4) / (pm5 - pm4) * (pm25 - pm4)) + aqi4;
            }
            else if (pm25 >= pm5 && pm25 <= pm6)
            {
                aqiPM25 = ((aqi6 - aqi5) / (pm6 - pm5) * (pm25 - pm5)) + aqi5;
            }
            else if (pm25 >= pm6 && pm25 <= pm7)
            {
                aqiPM25 = ((aqi7 - aqi6) / (pm7 - pm6) * (pm25 - pm6)) + aqi6;
            }
            else if (pm25 >= pm7 && pm25 <= pm8)
            {
                aqiPM25 = ((aqi8 - aqi7) / (pm8 - pm7) * (pm25 - pm7)) + aqi7;
            }

            return (int)aqiPM25;
        }

        public static int GetAQI10(double pm10)
        {
            const int pm1 = 0;
            const int pm2 = 54;
            const int pm3 = 154;
            const int pm4 = 254;
            const int pm5 = 354;
            const int pm6 = 424;
            const int pm7 = 504;
            const int pm8 = 604;

            const int aqi1 = 0;
            const int aqi2 = 50;
            const int aqi3 = 100;
            const int aqi4 = 150;
            const int aqi5 = 200;
            const int aqi6 = 300;
            const int aqi7 = 400;
            const int aqi8 = 500;

            var aqiPM10 = 0.0;

            if (pm10 >= pm1 && pm10 <= pm2)
            {
                aqiPM10 = ((aqi2 - aqi1) / (pm2 - pm1) * (pm10 - pm1)) + aqi1;
            }
            else if (pm10 >= pm2 && pm10 <= pm3)
            {
                aqiPM10 = ((aqi3 - aqi2) / (pm3 - pm2) * (pm10 - pm2)) + aqi2;
            }
            else if (pm10 >= pm3 && pm10 <= pm4)
            {
                aqiPM10 = ((aqi4 - aqi3) / (pm4 - pm3) * (pm10 - pm3)) + aqi3;
            }
            else if (pm10 >= pm4 && pm10 <= pm5)
            {
                aqiPM10 = ((aqi5 - aqi4) / (pm5 - pm4) * (pm10 - pm4)) + aqi4;
            }
            else if (pm10 >= pm5 && pm10 <= pm6)
            {
                aqiPM10 = ((aqi6 - aqi5) / (pm6 - pm5) * (pm10 - pm5)) + aqi5;
            }
            else if (pm10 >= pm6 && pm10 <= pm7)
            {
                aqiPM10 = ((aqi7 - aqi6) / (pm7 - pm6) * (pm10 - pm6)) + aqi6;
            }
            else if (pm10 >= pm7 && pm10 <= pm8)
            {
                aqiPM10 = ((aqi8 - aqi7) / (pm8 - pm7) * (pm10 - pm7)) + aqi7;
            }

            return (int)aqiPM10;
        }
    }
}