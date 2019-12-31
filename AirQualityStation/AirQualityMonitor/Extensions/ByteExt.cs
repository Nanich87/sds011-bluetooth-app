// Copyright (c) GNNMobile.eu. All rights reserved.

namespace AirQualityMonitor.Extensions
{
    using System;

    internal static class ByteExt
    {
        public static byte[] Decode(this byte[] packet)
        {
            if (packet.Length == 0)
            {
                return packet;
            }

            var i = packet.Length - 1;

            while (packet[i] == 0)
            {
                --i;
            }

            var temp = new byte[i + 1];
            Array.Copy(packet, temp, i + 1);

            return temp;
        }
    }
}