// Copyright (c) GNNMobile.eu. All rights reserved.

namespace AirQualityMonitor
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using AirQualityMonitor.Extensions;
    using Android.App;
    using Android.Bluetooth;
    using Android.Content;
    using Android.OS;
    using Android.Support.V4.App;
    using Java.Util;
    using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

    [Service(Enabled = true)]
    internal class BluetoothService : Service
    {
        public const string ServiceName = "BluetoothService";

        public const string KeyDevice = "device_address";

        public const string ActionStartService = "BluetoothService_ActionStartService";

        public const string ActionStopService = "BluetoothService_ActionStopService";

        public const string ActionDataReceived = "BluetoothService_ActionDataReceived";

        public const string KeyPM25 = "pm25";

        public const string KeyPM10 = "pm10";

        public const string KeyTemperature = "temperature";

        public const string KeyHumidity = "humidity";

        private const int ServiceRunningNotificationId = 1;

        private const string ServiceNotificationChannelId = "BluetoothService_NotificationChannelId";

        private const int ServiceReconnectDelay = 5000;

        private PowerManager.WakeLock wakeLock;

        private ConnectThread connectThread;

        private ConnectedThread connectedThread;

        private BluetoothDevice device;

        private List<byte> buffer;

        public static bool IsRunning { get; private set; }

        public IBinder Binder { get; private set; }

        public override void OnCreate()
        {
            base.OnCreate();

            CreateNotificationChannel();

            buffer = new List<byte>();
        }

        public override void OnDestroy()
        {
            Binder = null;

            DisposeThreads();

            ReleaseWakeLock();

            IsRunning = false;

            base.OnDestroy();
        }

        public override IBinder OnBind(Intent intent)
        {
            return Binder;
        }

        public override bool OnUnbind(Intent intent)
        {
            return base.OnUnbind(intent);
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case ActionStartService:
                    {
                        StartService(() =>
                        {
                            device = intent.GetParcelableExtra(KeyDevice) as BluetoothDevice;
                            Connect(device);
                        });

                        break;
                    }

                case ActionStopService:
                    {
                        StopService();

                        break;
                    }
            }

            return StartCommandResult.RedeliverIntent;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            if (GetSystemService(NotificationService) is NotificationManager notificationManager)
            {
                var name = Resources.GetString(Resource.String.channel_name);
                var description = GetString(Resource.String.channel_description);
                var channel = new NotificationChannel(ServiceNotificationChannelId, name, NotificationImportance.Low)
                {
                    Description = description,
                };

                notificationManager.CreateNotificationChannel(channel);
            }
        }

        private void AcquireWakeLock()
        {
            if (GetSystemService(PowerService) is PowerManager powerManager)
            {
                wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, ServiceName);
                wakeLock.Acquire();
            }
        }

        private void ReleaseWakeLock()
        {
            if (wakeLock != null && wakeLock.IsHeld)
            {
                wakeLock.Release();
            }
        }

        private void DisposeThreads()
        {
            if (connectThread != null)
            {
                connectThread.Cancel();
                connectThread = null;
            }

            if (connectedThread != null)
            {
                connectedThread.Cancel();
                connectedThread = null;
            }
        }

        private void StartService(Action action)
        {
            if (!IsRunning)
            {
                AcquireWakeLock();

                StartForeground(ServiceRunningNotificationId, CreateNotification(GetString(Resource.String.message_connecting_to_device)));

                IsRunning = true;

                action?.Invoke();
            }
        }

        private void StopService()
        {
            if (IsRunning)
            {
                StopForeground(true);
                StopSelf();
            }
        }

        private void Connect(BluetoothDevice device)
        {
            if (!IsRunning)
            {
                return;
            }

            DisposeThreads();

            BluetoothAdapter.DefaultAdapter.CancelDiscovery();

            connectThread = new ConnectThread(device);

            connectThread.OnConnected -= OnConnected;
            connectThread.OnConnected += OnConnected;

            connectThread.OnConnectionError -= OnConnectionError;
            connectThread.OnConnectionError += OnConnectionError;

            connectThread.Start();
        }

        private Notification CreateNotification(string contentText)
        {
            var intent = new Intent(this, typeof(MainActivity));
            intent.SetFlags(ActivityFlags.ClearTop);

            var stackBuilder = TaskStackBuilder.Create(this)
                                               .AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)))
                                               .AddNextIntent(intent);

            var pendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

            return new NotificationCompat.Builder(this, ServiceNotificationChannelId)
              .SetAutoCancel(true)
              .SetContentIntent(pendingIntent)
              .SetContentTitle(GetString(Resource.String.channel_description))
              .SetSmallIcon(Android.Resource.Drawable.StatSysDataBluetooth)
              .SetContentText(contentText)
              .SetVisibility((int)NotificationVisibility.Public)
              .Build();
        }

        private void Notify(Notification notification)
        {
            var notificationManager = NotificationManagerCompat.From(this);
            notificationManager.Notify(ServiceRunningNotificationId, notification);
        }

        private void OnConnected(BluetoothSocket socket, BluetoothDevice device)
        {
            connectThread = null;

            DisposeThreads();

            connectedThread = new ConnectedThread(socket);

            connectedThread.OnDataReceived -= OnDataReceived;
            connectedThread.OnDataReceived += OnDataReceived;

            connectedThread.OnDataError -= OnDataError;
            connectedThread.OnDataError += OnDataError;

            connectedThread.Start();

            Notify(CreateNotification(GetString(Resource.String.message_connected)));
        }

        private async void OnConnectionError(Exception ex)
        {
            Notify(CreateNotification(ex.Message));

            await Task.Delay(ServiceReconnectDelay);

            Connect(device);
        }

        private void OnDataReceived(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return;
            }

            bool parse = false;

            foreach (var item in data)
            {
                if (item != '\r')
                {
                    buffer.Add(item);
                }
                else
                {
                    parse = true;

                    break;
                }
            }

            if (!parse)
            {
                return;
            }

            string text = null;

            try
            {
                text = Encoding.UTF8.GetString(buffer.ToArray());
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            buffer.Clear();

            var fields = text.Split(',');
            if (fields.Length == 4)
            {
                var pm25 = double.Parse(fields[0], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                var pm10 = double.Parse(fields[1], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                var t = double.Parse(fields[2], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);
                var h = double.Parse(fields[3], NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture);

                Notify(CreateNotification(string.Format("PM2.5={0:F1} PM10={1:F1} T={2:F1}°C H={3:F0}%", pm25, pm10, t, h)));

                var intent = new Intent(ActionDataReceived);
                intent.PutExtra(KeyPM25, pm25);
                intent.PutExtra(KeyPM10, pm10);
                intent.PutExtra(KeyTemperature, t);
                intent.PutExtra(KeyHumidity, h);

                SendBroadcast(intent);
            }
        }

        private async void OnDataError(Exception ex)
        {
            Notify(CreateNotification(ex.Message));

            await Task.Delay(ServiceReconnectDelay);

            Connect(device);
        }
    }

    internal class ConnectThread : Java.Lang.Thread
    {
        private const string SppUuid = "00001101-0000-1000-8000-00805f9b34fb";

        private readonly BluetoothDevice device;

        private readonly BluetoothSocket socket;

        public ConnectThread(BluetoothDevice bluetoothDevice)
        {
            device = bluetoothDevice;

            BluetoothSocket tempSocket = null;

            try
            {
                tempSocket = bluetoothDevice.CreateRfcommSocketToServiceRecord(UUID.FromString(SppUuid));
            }
            catch (Java.IO.IOException)
            {
            }

            socket = tempSocket;
        }

        public delegate void ConnectedEventHandler(BluetoothSocket socket, BluetoothDevice device);

        public delegate void ConnectionErrorEventHandler(Exception ex);

        public event ConnectedEventHandler OnConnected;

        public event ConnectionErrorEventHandler OnConnectionError;

        public override void Run()
        {
            try
            {
                socket.Connect();
            }
            catch (Exception ex)
            {
                OnConnectionError?.Invoke(ex);

                return;
            }

            OnConnected?.Invoke(socket, device);
        }

        public void Cancel()
        {
            try
            {
                socket?.Close();
            }
            catch (Exception)
            {
            }
        }
    }

    internal class ConnectedThread : Java.Lang.Thread
    {
        private const int Buffer = 2048;

        private readonly BluetoothSocket socket;

        private readonly Stream inputStream;

        private volatile bool stopping;

        public ConnectedThread(BluetoothSocket bluetoothSocket)
        {
            socket = bluetoothSocket;
            stopping = false;

            Stream tmpIn = null;

            try
            {
                tmpIn = bluetoothSocket.InputStream;
            }
            catch (Java.IO.IOException)
            {
            }

            inputStream = tmpIn;
        }

        public delegate void DataReceivedEventHandler(byte[] data);

        public delegate void DataErrorEventHandler(Exception ex);

        public event DataReceivedEventHandler OnDataReceived;

        public event DataErrorEventHandler OnDataError;

        public override void Run()
        {
            while (!stopping)
            {
                if (inputStream == null)
                {
                    break;
                }

                byte[] data = new byte[Buffer];

                try
                {
                    if (!inputStream.IsDataAvailable())
                    {
                        System.Threading.Thread.Sleep(1);
                    }
                    else if (inputStream.Read(data, 0, data.Length) > 0)
                    {
                        OnDataReceived?.Invoke(data.Decode());
                    }
                    else
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    OnDataError?.Invoke(ex);

                    CloseSocket();

                    break;
                }
            }
        }

        public void Cancel()
        {
            stopping = true;

            CloseSocket();
        }

        private void CloseSocket()
        {
            try
            {
                socket.Close();
            }
            catch (Exception)
            {
            }
        }
    }
}